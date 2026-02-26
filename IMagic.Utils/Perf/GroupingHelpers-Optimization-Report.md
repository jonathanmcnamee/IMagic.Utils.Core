# GroupingHelpers & Search — Micro-allocation & Span Optimization Report

**Date:** 2025-02-26  
**Scope:** `GroupingHelpers.cs`, `SearchService.cs`, `DateGroupMetadata.cs`, `UiModels.cs`, `ExtensionMethods.cs`  
**Focus:** Span-based iteration, plain-array vs List trade-offs, eliminating redundant heap allocations.  
**Prior work:** GC reduction sprint (filter-hash, static cache options, enumerator elimination in `GetFilterHash`).

---

## 1. Executive Summary

This pass targets six independent, low-risk allocation hot-spots found across the search and grouping pipeline.
None require API surface changes or database schema modifications. Combined estimated savings per gallery page load:

| Category | Allocations eliminated | Heap bytes saved (est.) |
|---|---|---|
| Wasted `SearchFilter` alloc | 1 per Person/Camera group | ~400 B |
| `ObservableCollection` internal reallocs | 2 per view load (12+ groups) | ~192 B + copy cost |
| `DateTime` chain (endOfMonth) | 4 value-type method calls | 0 B heap, ~4 ns CPU |
| `GetDateGroupsAsync` list reallocs | 2–3 per gallery open | ~128 B + copy cost |
| `new DateTime()` for month name | 1 per group in DateGroupMetadata | ~48 B per group |
| `string[]`/`char[]` per-call allocations | 1–2 per file-size/GPS display | ~80–120 B per call |
| `ReadMany` list under-capacity | N reallocs for large channel reads | variable |

---

## 2. Findings

### 2.1 Wasted `SearchFilter` allocation in `CreateFilterForGroup`

**File:** `GroupingHelpers.cs`  
**Lines (before fix):** 37, 65–69, 82–86

**Problem:**  
`SearchFilter filter = new SearchFilter()` is unconditionally allocated at the top of the method.
For `GroupingMode.Person` and `GroupingMode.CameraModel`, the very next statement discards that object
and allocates a second `new SearchFilter { ... }`. The first object is immediately eligible for GC.

```csharp
// BEFORE — double alloc for Person and CameraModel branches
SearchFilter filter = new SearchFilter();      // alloc #1 (wasted for Person/Camera)
...
case GroupingMode.Person:
    filter = new SearchFilter { PersonIds = ... }; // alloc #2 replaces #1
```

**Fix:** Move construction inside each case; fall through to `return new SearchFilter()` only for the
no-metadata path. Each call site now allocates exactly once.

**Estimated saving:** 1 `SearchFilter` object (~400 B on heap including default collection fields) per
Person-grouped or Camera-grouped load cycle. A gallery with 20 person groups triggers 20 wasted allocs
on first load before caching.

---

### 2.2 `ObservableCollection<T>` without pre-sizing in `ToGroupedCollections`

**File:** `GroupingHelpers.cs`  
**Line (before fix):** 22

**Problem:**  
`ObservableCollection<T>` wraps an internal `List<T>`. With no capacity hint, that `List` starts at
capacity 4 and doubles (4 → 8 → 16 …). A typical date-grouped gallery with 12–36 month groups triggers
2–4 internal array reallocations and copies before the collection is handed to the UI.

**Fix (two parts):**  
1. Build a `List<GroupedImageCollection>` pre-sized to `groups.Count`.  
2. Pass it to `new ObservableCollection<T>(IList<T>)` — this constructor takes ownership of the list
   directly (no copy). Zero reallocations, one allocation.

**Additionally** — replace `foreach (ImageGroup group in groups)` with Span-based iteration:

```csharp
foreach (ImageGroup group in CollectionsMarshal.AsSpan(groups))
```

`CollectionsMarshal.AsSpan` returns a `ReadOnlySpan<ImageGroup>` over the list's internal array.
Compared to `List<T>`'s enumerator:
- No `List<T>.Enumerator` struct allocation (16 B, but GC-visible as part of the frame).
- `foreach` on `Span<T>` uses a plain index loop; the JIT eliminates the bounds check on the inner
  access because the span length is pinned — measurable in tight loops.
- No version-check branch on every `MoveNext()`.

**Estimated saving:** 2+ array reallocations + copy overhead eliminated per view load. Span iteration
saves ~1–3 ns per element in a tight loop (negligible at 36 groups, noticeable at 1000+).

---

### 2.3 Five-chain `DateTime.Add*()` for `endOfMonth`

**File:** `GroupingHelpers.cs`  
**Line (before fix):** 52

**Problem:**
```csharp
DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
```
Five separate `DateTime` struct method calls, each returning a new `DateTime` value. `DateTime` is a
value type so there are no heap allocations, but the chain is 5 method calls and 4 intermediate values
on the stack frame. More importantly the result is semantically "end of month at 23:59:59" which is
better expressed as a direct constructor:

```csharp
DateTime endOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
```

Single constructor call. Extracts `year` and `month` into locals first (avoids repeated null-coalescing
on `dateMetadata.Month ?? 1`).

**Estimated saving:** ~4 ns CPU per date-group filter creation. No heap impact (DateTime is a value
type). Primary benefit is clarity and reduced IL.

---

### 2.4 `List<ImageGroup>` without capacity in `GetDateGroupsAsync`

**File:** `SearchService.cs`  
**Line (before fix):** 612

**Problem:**
```csharp
var result = new List<ImageGroup>();
```
The EF Core query immediately before (`dateGroups`) is already materialized and has a known count.
The result list is filled in a `foreach` loop with `dateGroups.Count` items, possibly plus 1 for the
"Unknown Date" group. Starting at default capacity (4) causes 2–3 internal reallocations for a library
with more than 4 month-groups.

**Fix:**
```csharp
List<ImageGroup> result = new(dateGroups.Count + 1);
```
The `+ 1` reserves space for the potential "Unknown Date" sentinel group appended at the end.

**Estimated saving:** 2–3 list resizes and array copies eliminated per gallery load (cold path only;
result is cached afterwards). Approx ~128 B of short-lived array garbage per open.

---

### 2.5 `new DateTime()` allocation for month name in `DateGroupMetadata.ForYearMonth`

**File:** `DateGroupMetadata.cs`  
**Line (before fix):** 56

**Problem:**
```csharp
var monthName = new DateTime(year, month, 1).ToString("MMMM");
```
A full `DateTime` struct (8 B on stack) is created solely to call `.ToString("MMMM")`. This works but
is roundabout — `CultureInfo` already has a direct API for this:

```csharp
string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
```

`GetMonthName` is a direct array lookup into the culture's month-name table. No DateTime struct created,
no format string parsed, no calendar arithmetic. It also correctly respects the current UI culture
(same as `ToString("MMMM")` would).

**Estimated saving:** ~48 B stack pressure per call eliminated, plus the format-string parsing overhead
of `DateTime.ToString`. Called once per date group during grouping — 12–36 times per gallery open
(cold path only; result is cached).

---

### 2.6 Per-call `string[]` and `char[]` allocations in `ImageModel`

**File:** `UiModels.cs`

#### 2.6a `FormatFileSize` — `string[]` allocated on every call

**Line (before fix):** 384

```csharp
string[] sizes = { "B", "KB", "MB", "GB", "TB" };  // new string[5] every call
```

`FormatFileSize` is called each time a file's size is displayed in the detail panel, metadata list, or
any bound property that formats bytes. A new `string[5]` (80 B) is allocated on every invocation.

**Fix:** `private static readonly string[] _fileSizeSuffixes = { "B", "KB", "MB", "GB", "TB" };`

**Estimated saving:** 80 B per file-size display call. With 50 images visible in a gallery grid and
metadata panel open, this is ~4 KB avoided per navigation.

#### 2.6b `TryParseGPSCoordinate` — `char[]` separator allocated on every parse

**Line (before fix):** 347

```csharp
string[] parts = coordinate.Split(new[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
```

A `new char[2]` (40 B) is allocated on every GPS coordinate parse.

**Fix:** `private static readonly char[] _gpsSeparators = { ' ', '/' };`

**Estimated saving:** 40 B per GPS parse. Minor individually; relevant if many geotagged images are
displayed in quick succession.

---

### 2.7 `ReadMany` channel reader — list under-capacity

**File:** `ExtensionMethods.cs`  
**Line (before fix):** 59

**Problem:**
```csharp
List<T> results = new List<T>();
```
`maxCount` is the declared upper bound of results expected. Starting at capacity 4 wastes allocations
for callers that request batches larger than 4 items.

**Fix:**
```csharp
List<T> results = new List<T>(maxCount);
```

**Estimated saving:** Proportional to `maxCount`. For a batch of 50 channel items: 4 list resizes
(4→8→16→32→64) eliminated. Approx 4 short-lived array allocs + copy cost per batch read.

---

## 3. Array vs List Trade-off Analysis

### When a plain array beats `List<T>`

| Scenario | `List<T>` cost | `T[]` benefit |
|---|---|---|
| Fixed-size after creation | 24 B overhead (size + version + capacity) + backing array | Single object, no overhead fields |
| `foreach` iteration | Enumerator struct (16 B), version check per step | Index loop, JIT eliminates bounds check |
| `Span<T>` interop | Requires `CollectionsMarshal.AsSpan` | Direct `AsSpan(array)` or implicit |
| Interop / P/Invoke buffers | Must pin backing array separately | Pin directly |

### Candidates in this codebase

| Property | Current type | Suggested | Blocker |
|---|---|---|---|
| `ImageModel.Thumbnails` | `List<ThumbnailModel>` | `ThumbnailModel[]` | EF projection uses `.ToList()` → change to `.ToArray()`; callers use `.Count` (OK on arrays) |
| `ImageModel.MetadataItems` | `IReadOnlyList<ImageMetadataItemModel>` | `ImageMetadataItemModel[]` | As above; enables `CollectionsMarshal.AsSpan` in `GetAttributeValue` |
| `PersonModel.Faces` | `List<FaceModel>` | `FaceModel[]` | Small impact; low priority |

**Recommendation:** Migrate `ImageModel.Thumbnails` and `MetadataItems` to `T[]` in a dedicated
follow-up. At 1000 images per page, the combined saving is:  
`1000 images × 2 lists × ~40 B overhead = ~80 KB` of long-lived heap eliminated per page.  
Additionally, `GetAttributeValue` could use `MemoryExtensions.IndexOf` over a `ReadOnlySpan<T>` for
a branchless linear scan instead of `FirstOrDefault` with a delegate closure.

### `Span<T>` in this codebase — current opportunities

| Location | Current | Span opportunity |
|---|---|---|
| `ToGroupedCollections` foreach | `List<T>` enumerator | `CollectionsMarshal.AsSpan` ✅ (applied this sprint) |
| `GetAttributeValue` | `FirstOrDefault` with lambda | `CollectionsMarshal.AsSpan` on internal `List<T>`, manual loop — deferred |
| `ToByteArray` / `ToFloatArray` | `Buffer.BlockCopy` → `byte[]` | `MemoryMarshal.Cast<float, byte>(floatArray).ToArray()` — same result, marginally cleaner |
| EF projection `MetadataItems.Select(...).ToList()` | LINQ chain | N/A — LINQ over `IQueryable` stays as-is |

---

## 4. Changes Applied This Sprint

| # | File | Change |
|---|---|---|
| 1 | `GroupingHelpers.cs` | Fix double `SearchFilter` allocation — allocate once per branch |
| 2 | `GroupingHelpers.cs` | Pre-sized `List<T>` + `CollectionsMarshal.AsSpan` in `ToGroupedCollections` |
| 3 | `GroupingHelpers.cs` | Replace 5-chain `Add*()` with `DateTime(y,m,DaysInMonth,23,59,59)` |
| 4 | `SearchService.cs` | `new List<ImageGroup>(dateGroups.Count + 1)` in `GetDateGroupsAsync` |
| 5 | `DateGroupMetadata.cs` | `GetMonthName(month)` replaces `new DateTime(...).ToString("MMMM")` |
| 6 | `UiModels.cs` | `static readonly string[] _fileSizeSuffixes` (was per-call local) |
| 7 | `UiModels.cs` | `static readonly char[] _gpsSeparators` (was per-call `new[]`) |
| 8 | `ExtensionMethods.cs` | `new List<T>(maxCount)` in `ReadMany` |

---

## 5. Not Applied — Deferred

| Item | Reason |
|---|---|
| `ImageModel.Thumbnails` → `T[]` | Wide API surface change; needs dedicated review |
| `ImageModel.MetadataItems` → `T[]` | Same; enables `Span` in `GetAttributeValue` |
| `GetAttributeValue` Span scan | Blocked on above |
| `ToByteArray` → `MemoryMarshal.Cast` | No allocated-copy savings unless callers accept `ReadOnlySpan<byte>` |

---

## 6. Regression Checklist

- [ ] `CreateFilterForGroup` date-unknown sentinel path still sets `To = new DateTime(1899, 12, 31)`
- [ ] `CreateFilterForGroup` returns default `new SearchFilter()` for unknown/unmatched modes
- [ ] `ToGroupedCollections` output count matches input group count
- [ ] `endOfMonth` for February in a leap year is `29` (validated by `DateTime.DaysInMonth`)
- [ ] Month name locale matches previous output (both use `CurrentCulture`)
- [ ] `ObservableCollection` handed to UI is still mutable (OC wrapping a `List<T>` remains mutable)
