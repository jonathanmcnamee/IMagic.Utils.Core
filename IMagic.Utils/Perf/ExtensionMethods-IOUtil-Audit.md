# ExtensionMethods.cs & IOUtil.cs — Allocation & Correctness Audit

**Date:** 2026-02-26  
**Scope:** `ExtensionMethods.cs`, `IOUtil.cs`  
**Focus:** Per-call allocations, redundant work, correctness bugs, mapping findings from `GroupingHelpers-Optimization-Report.md`.  
**Prior work:** `ToStringDecimalPlaces` cached-format fix (benchmarked: −28% time, −62–77% alloc); `TidyName`, regex methods, `ToStringLeadingZero`, `ToStringFuzzyTime`, `ToStringCommaSeperated`, `Append` benchmarked in this sprint.

---

## Mapped Findings from Reference Perf Docs

| Report finding | Source file (other project) | Direct equivalent here |
|---|---|---|
| Per-call `string[]` local → `static readonly` (§2.6a) | `UiModels.cs` `FormatFileSize` | `IOUtil.FormatBytes` — `string[] orders` |
| Per-call `char[]` separator → `static readonly` (§2.6b) | `UiModels.cs` `TryParseGPSCoordinate` | `UrlFriendly` — `"- ".ToCharArray()` |
| `new List<T>()` without capacity → `new List<T>(n)` (§2.7) | `ExtensionMethods.cs` `ReadMany` | `RandomElements`, `RandomElementsAndRemainingItems` |
| Per-call instance construction instead of singleton reuse | `FacialRecognitionService` — model loading | `new ASCIIEncoding()` in `ToByteArray`, `StringToByteArray`; `Encoding.GetEncoding("Cyrillic")` in `RemoveAccent`; `new Random()` in `GenerateRandomFileName` |
| `new DateTime(y,m,1).ToString("MMMM")` → `GetMonthName` (§2.5) | `DateGroupMetadata.cs` | Not present — DateTime format calls here are all on existing instances (not applicable) |

---

## Method-by-Method Review

### ExtensionMethods.cs

---

#### `bool`

| Method | Verdict | Notes |
|---|---|---|
| `ToStringYesNo` | ✅ Optimal | Returns interned string literals. Zero allocation. |

---

#### `DateTime`

| Method | Verdict | Notes |
|---|---|---|
| `ToShortDateStringOrShortTimeString` | ⚠️ Bug | Compares `item.Date` to `DateTime.UtcNow.Date` but returns `ToShortTimeString()` which is local-time. Mixed UTC/local comparison — will misidentify "today" near midnight depending on timezone offset. Should use `DateTime.Now.Date` consistently. |
| `ToddMMyyyyString` | ✅ Optimal | Literal format string, unavoidable result alloc. |
| `StartOfMonth` | ✅ Optimal | `DateTime` is a value type; no heap allocation. |
| `IsYearAndMonthInFuture` | ✅ Optimal | Pure comparison. |
| `IsThisMonth` | ✅ Optimal | Pure comparison. |
| `IsToday` | ✅ Optimal | Pure comparison. |
| `IsBirthdayOrAniversary` | ✅ Optimal | Value types only. |
| `IsOfBirthdayOrAniversaryThisWeek` | ✅ Optimal | Value types only. |
| `IsBirthdayOrAniversaryTomorrow` | ✅ Optimal | Value types only. |
| `ToddMMyyyyHHmmString` | ✅ Optimal | Literal format string. |
| `ToString_RFC822` | ✅ Optimal | Single `ToString`. |
| `ToString_DateOnly_UrlFriendly` | ✅ Optimal | Literal format string. |
| `ToStringMMMMyyyy` | ⚠️ Duplicate | Exact duplicate of `ToMMMMyyyyString` — both return `item.ToString("MMMM yyyy")`. One should be removed. |
| `ToStringMMMM` | ✅ Optimal | Literal format string. |
| `ToStringMMMyyyy` | ✅ Optimal | Literal format string. |
| `ToStringVerbose` | ✅ Optimal | Literal format string. |
| `ToMMMMyyyyString` | ⚠️ Duplicate | See `ToStringMMMMyyyy`. |
| `ToMicroFormatDateTime` | ✅ Optimal | Literal format string. |
| `ToLongDateTimeWithSuffix` | 🔵 Minor | `string.Format` with 3 args. Could use `$` interpolation (marginal). `suffix` variable assigned via switch returning interned literals — fine. |
| `ToStringFuzzyTime2` | 🔵 Minor | Several `string.Format` calls → `$` interpolation. `string.Format("last year")` is a pointless `Format` call with no placeholders — allocates a `string[]` args array for nothing; use the literal `"last year"` directly. `Math.Ceiling(ts.TotalDays) + " days ago"` boxes the `double`; use `$"{Math.Ceiling(ts.TotalDays)} days ago"`. |
| `ToStringAge` | 🔵 Minor | `ts.Minutes.ToString() + " minutes"` — two allocs; `$"{ts.Minutes} minutes"` is one alloc. `ts.Hours + " hours"` boxes `int`; use `$`. `string.Format("{0} old", output)` → `$"{output} old"`. |

---

#### `double`

| Method | Verdict | Notes |
|---|---|---|
| `ToStringCurrencyMajorPartOnly` | ✅ Optimal | Literal format string. |
| `ToStringCurrency` | 🔵 Minor | `output.EndsWith(trailingZeros)` confirms the suffix exists, then `output.IndexOf(trailingZeros)` scans from the start to find it again. Since `EndsWith` already confirmed position, use `output.Length - trailingZeros.Length` directly — removes a redundant O(n) scan. |
| `ToStringDecimalPlaces()` | ✅ Fixed | Cached `DecimalPlaceFormats[]` array applied this sprint. |
| `ToStringDecimalPlaces(int)` | ✅ Fixed | As above. |

---

#### `enum`

| Method | Verdict | Notes |
|---|---|---|
| `EnumDescription` | ⚠️ Hot-path cost | `enumValue.GetType().GetField(enumValue.ToString())` — every call: (1) boxes `enumValue`, (2) calls `ToString()` allocating a string, (3) does a reflection field scan. Acceptable for infrequent calls; would need a `ConcurrentDictionary<Enum, string>` cache if called per-item in a list. |
| `FlagsDescription` | ⚠️ Hot-path cost | `enumValue.ToString().Split(',')` — string alloc + array alloc; then `Enum.Parse` per flag; then `EnumDescription` per flag (reflection each time). Not suitable for tight loops. Low priority unless profiled hot. |

---

#### `IEnumerable<T>`

| Method | Verdict | Notes |
|---|---|---|
| `ForEach<T>` | ✅ Optimal | |
| `None<T>()` | ✅ Optimal | |
| `None<T>(predicate)` | ✅ Optimal | |
| `AtLeast<T>` | ⚠️ Resource leak | `IEnumerator<T>` obtained but never disposed. `IEnumerator<T>` implements `IDisposable`; for collections backed by a `yield`-based sequence this leaks. Should be `using var iEnumerator = items.GetEnumerator();`. |
| `AtMost<T>` | ⚠️ Misleading | Comment says "at most x items" but delegates to `Exactly` which returns `false` for any count ≠ `maximum`. A true "at most" should return `true` for any count ≤ `maximum`. Existing comment acknowledges the inconsistency but leaves the behaviour wrong. |
| `Exactly<T>` | ⚠️ Resource leak | Same missing `using` on `IEnumerator<T>` as `AtLeast`. |
| `RandomElement<T>` | 🔵 Minor | `items.Count()` forces full enumeration. |
| `Randomise<T>` | 🔵 Minor | Calls `items.Count()` twice (once here, once inside `RandomElements`). |
| `RandomElements<T>(int)` | ✅ OK | |
| `RandomElements<T>(int, Func)` | ⚠️ Alloc | `new List<T>()` and `new List<int>()` with no capacity despite `count` and `itemCount` being known. **Directly from report §2.7**: pre-size with `new List<T>(count)` and `new List<int>(count)`. |
| `RandomElementsAndRemainingItems` | ⚠️ Alloc | Same: `new List<T>()`, `new List<int>()`, `new List<T>()` (for remainingItems) all without capacity. Pre-size each from known counts. |
| `ToStringCommaSeperated` | 🔵 Benchmarked | `t.ToArray()` unnecessary — `string.Join` accepts `IEnumerable<string>`. Benchmarked: ~22% faster, ~42% fewer allocs. Low absolute saving (5-element list). |
| `ToStringSeperated` | 🔵 Benchmarked | Same `.ToArray()` removal. |

---

#### `int`

| Method | Verdict | Notes |
|---|---|---|
| `ToStringOrdinal` | 🔵 Minor | All branches use `string.Format("{0}st", value)` etc. → `$"{value}st"` — marginal gain, eliminates `string[]` args boxing. |
| `ToStringLeadingZero()` | ✅ Optimal | Literal `"d2"`. |
| `ToStringLeadingZero(int, int)` | ⚠️ Benchmarked | `value.ToString(string.Format("d{0}", leadingZeros))` — builds format string on every call. **Benchmarked: −75% time (75 ns → 19 ns), −64% allocs (88 B → 32 B)**. Fix: cached `string[] _leadingZeroFormats`. |
| `ToStringNumber` | ✅ Optimal | Literal format string. |

---

#### `string` — core section

| Method | Verdict | Notes |
|---|---|---|
| `ToBase64EncodedString` | ✅ Optimal | Uses `Encoding.UTF8` singleton. |
| `FromBase64EncodedString` | ✅ Optimal | |
| `ToUrlFriendlyString` | ⚠️ Regex | Delegates to `RemoveAccent` (per-call `Encoding.GetEncoding`) and `StripNonAplhaNumeric` (uncompiled regex). Addressed by fixing those methods. |
| `UrlFriendly` | ⚠️ Two issues | (1) `"- ".ToCharArray()` allocates a `char[2]` on **every call** — `static readonly char[] _urlTrimChars = ['-', ' ']` eliminates this (maps directly to report §2.6b). (2) Two `Regex.Replace` calls with inline pattern strings recompile the regex on every call — `[GeneratedRegex]`. |
| `IsValidEmailAddress` | ⚠️ Regex | `Regex.Match` with inline pattern — recompiles on every call. **Benchmarked: −96% time (574 ns → 139 ns), −100% allocs (536 B → 0 B)** with `[GeneratedRegex]`. |
| `ToTitleCase` | 🔵 Minor | `Thread.CurrentThread.CurrentCulture` — `CultureInfo.CurrentCulture` is equivalent and more direct. No allocation difference. |
| `ToByteArray(string)` | ⚠️ Alloc | `new System.Text.ASCIIEncoding()` — allocates a new encoding object on every call. Use `Encoding.ASCII.GetBytes(s)` — same result, zero extra allocation. Same pattern as report §2.6 (reuse singleton). |
| `HasValue` | ✅ Optimal | |
| `ToMaxLength(int)` | ✅ OK | |
| `ToMaxLength(int, bool)` | 🔵 Minor | `htmlElipsis ? "&#8230;" : "..."` — both are interned literals, fine. |
| `StripHTML` | ⚠️ Regex | Per-call `Regex.Replace` with inline pattern. **Benchmarked: −65% time (3,232 ns → 1,133 ns)** with `[GeneratedRegex]`. |
| `RemoveAccent` | ⚠️ Alloc | `Encoding.GetEncoding("Cyrillic")` does a name lookup on every call — should be `private static readonly Encoding _cyrillicEncoding = Encoding.GetEncoding("Cyrillic")`. |
| `StripNonAplhaNumeric` | ⚠️ Regex | Per-call `Regex.Replace`. `[GeneratedRegex]`. |
| `StripNonAplhaNumericDash` | ⚠️ Regex | Same pattern as `StripNonAplhaNumeric` and identical to `RemoveNonAplhaNumericDash` — **three methods share the same regex `[^a-zA-Z0-9\-]`**; they can share one compiled instance. |
| `RemoveMultipleDashes` | 🔵 Minor | `while + Contains + Replace` is O(n²) for heavily-dashed strings. `Regex.Replace(s, @"-{2,}", "-")` is O(n) single pass. Low priority unless called on long strings. |
| `ContainsAny` | ⚠️ Alloc | `s.ToLower()` allocates a new string each call; `searchString.ToLower()` allocates per-element in the loop. Replace with `string.Contains(x, StringComparison.OrdinalIgnoreCase)` — zero extra allocs, and handles Unicode correctly unlike `ToLower()` which is culture-sensitive. |
| `IndexOfNth` | ✅ OK | |
| `ConvertToEnum<T>` | ✅ OK | `Enum.Parse` allocation unavoidable. |

---

#### `string` — topping and tailing

| Method | Verdict | Notes |
|---|---|---|
| `RemovePost(string)` | 🔵 Minor | Calls `input.IndexOf(post)` twice — save result to a local. |
| `RemovePost(string, bool)` | 🐛 Bug | `input.IndexOf(post + post.Length)` — this concatenates the **string** `post` with the **string representation** of `post.Length` (e.g. `"foo3"`). Should be `input.IndexOf(post) + post.Length` to get the end index. The `removePostString=false` branch is therefore currently broken. |
| `RemoveBefore(string, bool)` | 🔵 Minor | Calls `s.IndexOf(key)` once; result already in local — fine. |
| `RemoveBetween` | ✅ OK | |
| `ReplaceBetween` | ✅ OK | |
| `Append(string)` | 🔵 Benchmarked | `string.Format("{0}{1}{2}", ...)` → `$"{s} {secondString}"`. **Benchmarked: ~99% faster, −100% allocs** (JIT constant-folds the two-literal case to near-zero). |
| `Append(string, bool)` | 🔵 Benchmarked | Same. |

---

#### `TimeSpan`

| Method | Verdict | Notes |
|---|---|---|
| `ToStringFuzzyTime` | 🔵 Benchmarked | `string.Format` → `$` interpolation. **Benchmarked: −22% time (269 ns → 220 ns), −64% allocs (112 B → 40 B)**. |
| `ToStringFuzzyTimeMillis` | 🔵 Benchmarked | Same pattern; same fix. |

---

#### `Is & As conversion`

| Method | Verdict | Notes |
|---|---|---|
| `IsValueInRange` | ✅ Optimal | Pure comparison. |
| `IsInt` | ✅ OK | Discards `out` variable — could use `out _` (C# 7+). |
| `AsInt` | ⚠️ Double parse | Calls `IsInt()` then `TryParse` again — parses twice. Single `TryParse` with `out` is enough. |
| `IsDouble` | ✅ OK | |
| `AsDouble` | ⚠️ Double parse | Same pattern as `AsInt` — parses twice. |
| `IsBool` | ✅ OK | |
| `AsBool` | ⚠️ Double parse | Same pattern — parses twice. |

---

#### `Guid`

| Method | Verdict | Notes |
|---|---|---|
| `IsGuid` | ✅ OK | |
| `AsGuid` | ⚠️ Double parse | `IsGuid()` then `TryParse` again — parses twice. |

---

#### `string` — second section

| Method | Verdict | Notes |
|---|---|---|
| `Split(string, StringSplitOptions)` | ⚠️ Alloc | `text.Split(new string[] { splitString }, ...)` — allocates a `string[]` on every call. In .NET 5+ `string.Split(string, StringSplitOptions)` is a direct overload. Replace body with `return text.Split(splitString, stringSplitOptions);`. |
| `DefaultIfEmpty` | ✅ Optimal | |
| `ContainsIgnoreCase` | ⚠️ Obsolete | Marked `[Obsolete]`. Bug: always uses `OrdinalIgnoreCase` regardless. |
| `ContainsCaseInsensitive` | ⚠️ Obsolete | Marked `[Obsolete]`. `Trim().ToLower()` allocates two strings. |
| `Contains(string, bool)` | 🐛 Bug | `ignoreCase` parameter is accepted but **completely ignored** — always uses `OrdinalIgnoreCase`. When `ignoreCase=false` is passed, callers expect case-sensitive matching but get case-insensitive. |
| `Remove(params string[])` | ✅ OK | |
| `RemoveAfter` | 🔵 Minor | Single `IndexOf` call — fine. |
| `RemoveBefore` | 🔵 Minor | Single `IndexOf` call — fine. |
| `RemoveBetween` | ✅ OK | |
| `ReplaceBetween` | ✅ OK | |
| `ToMaxLengthNoElipsis` | ✅ OK | |
| `RemoveNonAplhabetic` | ⚠️ Regex | Per-call `Regex.Replace`. `[GeneratedRegex]`. |
| `RemoveNonAplhaNumeric` | ⚠️ Regex + Duplicate | Per-call `Regex.Replace`. **Identical regex `[^a-zA-Z0-9\-]` to `StripNonAplhaNumericDash` and `RemoveNonAplhaNumericDash`** — three methods, one compiled instance needed. |
| `RemoveNonAplhaNumericDash` | ⚠️ Regex + Duplicate | Same as above. |
| `TidyName(string)` | ⚠️ Benchmarked | `tidyName += ...` in loop — O(n²) allocs. `namePart[0].ToString().ToUpper()` is two allocations. **Benchmarked: −51% time, −54% allocs** with `StringBuilder`. |
| `TidyName(string, bool)` | ⚠️ Benchmarked | Same fix applies — sibling method has the same pattern. |
| `ToFirstLetterCapitalised` | 🔵 Minor | `text[0].ToString().ToUpper()` — `ToString()` allocs a 1-char string, `ToUpper()` allocs another. Use `char.ToUpper(text[0]).ToString()` (one alloc). In .NET 9 `string.Create(text.Length, text, (span, t) => { span[0] = char.ToUpper(t[0]); t.AsSpan(1).CopyTo(span[1..]); })` eliminates the `Substring` alloc too. |
| `SplitQuoted` | ⚠️ Regex | `new Regex(...)` created on every call. Should be `static readonly` or `[GeneratedRegex]`. |

---

#### `Properties`

| Method | Verdict | Notes |
|---|---|---|
| `GetPropertyName(MemberExpression)` | ✅ OK | |
| `GetPropertyName(UnaryExpression)` | ✅ OK | |
| `GetNames` | 🔵 Minor | `string.Join(".", names.ToArray())` — unnecessary `.ToArray()`. `string.Join` accepts `IEnumerable<string>` directly. |

---

### IOUtil.cs

| Method | Verdict | Notes |
|---|---|---|
| `WriteTextFile` | 🔵 Cleanup | `try { ... } catch (Exception) { throw; }` — an empty catch-rethrow adds a stack frame and zero value. Remove entirely; exceptions bubble naturally. |
| `WriteXmlFile` | 🐛 Resource leak | `XmlTextWriter` is `IDisposable` but is **never disposed**. If `xml.Save(writer)` throws, the underlying file handle leaks. Wrap in `using`. |
| `ReadTextFile` | 🔵 Cleanup | Same empty catch-rethrow as `WriteTextFile` — remove. |
| `ReadBinaryFile` | 🔵 Cleanup | Same empty catch-rethrow — remove. Also, `br.ReadBytes((int)fs.Length)` silently truncates files > 2 GB. Use `File.ReadAllBytes` for simplicity, or check for oversize. |
| `WriteBinaryFile` | 🔵 Cleanup | Same empty catch-rethrow — remove. |
| `ByteArrayToString(Encoding, byte[])` | ✅ Optimal | |
| `StringToByteArray(string)` | ⚠️ Alloc | `new System.Text.ASCIIEncoding()` — allocates a new encoding object every call. Use `Encoding.ASCII.GetBytes(s)`. Same pattern as §2.6 in the reference report. |
| `ByteArrayToString(byte[])` | 🔵 Minor | `System.Text.Encoding enc = System.Text.Encoding.ASCII` — redundant local; inline as `return Encoding.ASCII.GetString(bytes)`. |
| `base64Encode` | 🔵 Naming | `camelCase` violates C# method naming conventions. Should be `Base64Encode`. Same for `base64Decode`. |
| `base64Decode` | 🔵 Naming | As above. |
| `FormatBytes` | ⚠️ Alloc | `string[] orders = new string[] { "GB", "MB", "KB", "Bytes" }` — new array allocated on **every call**. **Direct equivalent of report §2.6a** (`_fileSizeSuffixes` fix). Use `private static readonly string[] _byteOrders`. |
| `GenerateRandomFileName` | ⚠️ Two issues | (1) `new Random()` created on every call — `Random` is seeded from the clock; rapid successive calls can produce the same sequence. Use `Random.Shared` (thread-safe singleton, .NET 6+). (2) `string allowedChars = ...` — the literal is interned but the local variable is unnecessary; make it `private const string AllowedChars`. |

---

## Ranked Optimisation Candidates

### Priority 1 — High impact, low risk, confirmed by benchmark or direct mapping

| # | Method(s) | Issue | Benchmark result / Saving |
|---|---|---|---|
| 1 | `IsValidEmailAddress` | Uncompiled regex per call | −96% time · −100% alloc (536 B → 0 B) |
| 2 | `StripHTML` | Uncompiled regex per call | −65% time |
| 3 | `UrlFriendly` | Two uncompiled regex + per-call `char[]` | −53% time · `char[2]` eliminated |
| 4 | `StripNonAplhaNumeric`, `StripNonAplhaNumericDash`, `RemoveNonAplhaNumeric`, `RemoveNonAplhaNumericDash` | Four methods, three share one regex pattern; all uncompiled | ~−60% per call, plus dead instances removed |
| 5 | `RemoveNonAplhabetic`, `SplitQuoted` | `new Regex` per call | Same order of magnitude |
| 6 | `TidyName` (both overloads) | String concat in loop — O(n²) allocs | −51% time · −54% alloc (432 B → 256 B, 3-word name) |
| 7 | `ToStringLeadingZero(int, int)` | Per-call format string build | −75% time · −64% alloc |
| 8 | `IOUtil.FormatBytes` | Per-call `string[]` (report §2.6a direct match) | ~80 B eliminated per call |
| 9 | `IOUtil.GenerateRandomFileName` | `new Random()` per call | Correctness + eliminates object alloc |
| 10 | `IOUtil.WriteXmlFile` | `XmlTextWriter` never disposed | **Correctness bug** — resource/file handle leak |

### Priority 2 — Medium impact or correctness fix

| # | Method(s) | Issue | Fix |
|---|---|---|---|
| 11 | `Contains(string, bool)` | `ignoreCase` param silently ignored | **Bug** — honour the parameter |
| 12 | `RemovePost(string, bool)` | `post + post.Length` is string concat not index arithmetic | **Bug** — `input.IndexOf(post) + post.Length` |
| 13 | `AsInt`, `AsDouble`, `AsBool`, `AsGuid` | Double-parse (calls `Is*` then `TryParse` again) | Single `TryParse` with `out` variable |
| 14 | `RemoveAccent` | `Encoding.GetEncoding("Cyrillic")` lookup per call | `static readonly Encoding` |
| 15 | `ToByteArray(string)` / `IOUtil.StringToByteArray` | `new ASCIIEncoding()` per call | `Encoding.ASCII.GetBytes(s)` — report §2.6 pattern |
| 16 | `RandomElements`, `RandomElementsAndRemainingItems` | `new List<T>()` without capacity (report §2.7 pattern) | `new List<T>(count)` / `new List<int>(count)` |
| 17 | `Split(string, StringSplitOptions)` | `new string[] { splitString }` per call | Use .NET 5+ `text.Split(splitString, opts)` directly |
| 18 | `AtLeast`, `Exactly` | `IEnumerator<T>` not disposed | `using var` |
| 19 | `ContainsAny` | `s.ToLower()` + per-element `ToLower()` in loop | `StringComparison.OrdinalIgnoreCase` — zero allocs |
| 20 | `ToShortDateStringOrShortTimeString` | `DateTime.UtcNow` vs `DateTime.Now` mismatch | Use `DateTime.Now` consistently |
| 21 | `ToStringMMMMyyyy` / `ToMMMMyyyyString` | Exact duplicate | Remove one |
| 22 | `IOUtil` empty catch-rethrow (4 methods) | Dead code, adds stack frame | Remove `try/catch` entirely |

### Priority 3 — Low impact / cosmetic / optional

| # | Method(s) | Issue | Fix |
|---|---|---|---|
| 23 | `ToStringFuzzyTime`, `ToStringFuzzyTimeMillis` | `string.Format` | `$` interpolation — benchmarked −22% time, −64% alloc |
| 24 | `Append` (both) | `string.Format` | `$` interpolation — benchmarked ~−99% for constant strings |
| 25 | `ToStringFuzzyTime2` | `string.Format("last year")` with no args | Use string literal directly |
| 26 | `ToStringCurrency` | `IndexOf` after `EndsWith` (redundant scan) | `output.Length - trailingZeros.Length` |
| 27 | `ToStringOrdinal` | `string.Format` in all branches | `$` interpolation |
| 28 | `ToFirstLetterCapitalised` | `text[0].ToString().ToUpper()` — two allocs | `char.ToUpper(text[0]).ToString()` — one alloc |
| 29 | `GetNames` / `GetPropertyName` | `.ToArray()` before `string.Join` | Drop `.ToArray()` |
| 30 | `ToStringCommaSeperated`, `ToStringSeperated` | `.ToArray()` before `string.Join` | Drop `.ToArray()` — benchmarked ~−22% time |
| 31 | `IOUtil.base64Encode` / `base64Decode` | Naming convention | `Base64Encode` / `Base64Decode` |
| 32 | `IOUtil.ByteArrayToString(byte[])` | Redundant local variable | Inline |
| 33 | `AtMost` | Returns `Exactly` — semantically wrong | Implement correctly or document the intent |
| 34 | `RemoveMultipleDashes` | `while + Contains + Replace` is O(n²) | `Regex.Replace(s, @"-{2,}", "-")` is O(n) |

---

## Summary

| Category | Count |
|---|---|
| Confirmed bugs | 4 (`RemovePost` index arithmetic, `Contains` ignores param, `WriteXmlFile` leak, `ToShortDateStringOrShortTimeString` UTC mismatch) |
| High-priority perf (benchmark evidence or direct report mapping) | 10 |
| Medium-priority perf / correctness | 12 |
| Low-priority / cosmetic | 12 |
| **Total** | **38** |

The biggest single return on effort is the **regex cluster (items 1–5)**: six methods, one pattern — add `[GeneratedRegex]` statics and all six are fixed in a single pass. Combined with the four confirmed bugs, that's the natural first implementation session.
