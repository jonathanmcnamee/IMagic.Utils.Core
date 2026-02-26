# IMagic.Utils.Core

[![NuGet](https://img.shields.io/nuget/v/IMagic.Utils.Core.svg?color=blue&label=NuGet)](https://www.nuget.org/packages/IMagic.Utils.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/IMagic.Utils.Core.svg?color=green)](https://www.nuget.org/packages/IMagic.Utils.Core)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![Build](https://github.com/jonathanmcnamee/IMagic.Utils.Core/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/jonathanmcnamee/IMagic.Utils.Core/actions/workflows/nuget-publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-brightgreen.svg)](LICENSE)

A focused, zero-dependency utility library for .NET 9 — a battle-tested set of extension methods, I/O helpers, cryptography wrappers, geography utilities, and test-data generators that eliminate the same boilerplate across every project.

---

## Table of Contents

- [Installation](#installation)
- [What's inside](#whats-inside)
  - [Extension Methods](#extension-methods)
  - [IOUtil — File & Directory I/O](#ioutil--file--directory-io)
  - [CryptoUtil — Hashing](#cryptoutil--hashing)
  - [RandomUtil — Randomisation](#randomutil--randomisation)
  - [FakeDataUtil — Test Data](#fakedatautil--test-data)
  - [GeoUtil — Geography & Coordinates](#geoutil--geography--coordinates)
  - [DirectoryHelper — Path Helpers](#directoryhelper--path-helpers)
  - [DriveUtil — Drive Information](#driveutil--drive-information-windows)
- [CI / Publishing](#ci--publishing)
- [License](#license)

---

## Installation

```bash
dotnet add package IMagic.Utils.Core
```

All types live in the **global namespace** — no `using` statements required.

---

## What's inside

### Extension Methods

A large, well-organised collection covering the most common .NET types.

#### `bool`

```csharp
true.ToStringYesNo()   // "Yes"
false.ToStringYesNo()  // "No"
```

#### `DateTime`

```csharp
DateTime.Now.ToShortDateStringOrShortTimeString() // "14:32" if today, "01/06/2025" otherwise
DateTime.Now.ToStringFuzzyTime2()                 // "3 minutes ago"
DateTime.Now.AddYears(-25).ToStringAge()          // "25 years old"
DateTime.Now.ToddMMyyyyString()                   // "01/06/2025"
DateTime.Now.ToStringVerbose()                    // "Sunday, June 01, 2025"
DateTime.Now.IsBirthdayOrAniversary()             // true if today matches the day/month
```

#### `double`

```csharp
3.14159.ToStringDecimalPlaces()     // "3.14"
3.14159.ToStringDecimalPlaces(4)    // "3.1416"
1234.5.ToStringCurrency()           // "$1,234.50"
```

#### `Enum`

```csharp
// given: [Description("Super Admin")] SuperAdmin
MyRole.SuperAdmin.EnumDescription()  // "Super Admin"
myFlagsValue.FlagsDescription()      // comma-separated descriptions of each set flag
```

#### `IEnumerable<T>`

```csharp
items.ForEach(x => Console.WriteLine(x));
items.None()               // true when empty
items.AtLeast(3)           // true when count >= 3  (no full enumeration)
items.Exactly(5)           // true when count == 5
items.RandomElement()      // one random item
items.Randomise()          // shuffled copy
items.RandomElements(10)   // 10 distinct random items

names.ToStringCommaSeperated()   // "Alice,Bob,Charlie"
names.ToStringSeperated(" | ")   // "Alice | Bob | Charlie"
```

#### `int`

```csharp
1.ToStringOrdinal()         // "1st"
42.ToStringLeadingZero()    // "42"
5.ToStringLeadingZero(3)    // "005"
1234567.ToStringNumber()    // "1,234,567"
```

#### `string`

```csharp
"hello world".ToUrlFriendlyString()        // "hello-world"
"user@example.com".IsValidEmailAddress()   // true
"  ".HasValue()                            // false
"apple".ToFirstLetterCapitalised()         // "Apple"
"hello".ToBase64EncodedString()            // "aGVsbG8="
"aGVsbG8=".FromBase64EncodedString()       // "hello"
"123456789".RemoveAfter("6")               // "789"
"123456789".RemoveBefore("6")              // "12345"
"123456789".RemoveBetween("3", "7")        // "1289"
"123456789".ReplaceBetween("3", "7", "X") // "12X89"
"Some <b>HTML</b>".StripHTML()             // "Some HTML"
"café".RemoveAccent()                      // "cafe"
"hello world".ToTitleCase()               // "Hello World"
"123".IsInt()                              // true
"3.14".AsDouble()                          // 3.14
```

#### `float[]` / `byte[]`

```csharp
float[] floats = new[] { 1.0f, 2.0f, 3.0f };
byte[]  bytes  = floats.ToByteArray();   // Buffer.BlockCopy to bytes
float[] back   = bytes.ToFloatArray();   // round-trip back to floats
```

#### `ChannelReader<T>`

```csharp
// Read up to 50 items from a channel in one async batch
List<MyMessage> batch = await channel.Reader.ReadMany(50, cancellationToken);
```

#### `DirectoryInfo`

```csharp
new DirectoryInfo(@"C:\temp\work").ClearDirectory(); // deletes all files and sub-directories
```

---

### IOUtil — File & Directory I/O

```csharp
// Text
IOUtil.WriteTextFile("log.txt", "Hello", append: true);
string text = IOUtil.ReadTextFile("log.txt");

// XML
IOUtil.WriteXmlFile("config.xml", xmlDoc);

// Binary
byte[] data = IOUtil.ReadBinaryFile("image.bin");
IOUtil.WriteBinaryFile("copy.bin", data);

// Base64
string encoded = IOUtil.Base64Encode("secret");
string decoded = IOUtil.Base64Decode(encoded);

// Byte conversion
string s = IOUtil.ByteArrayToString(Encoding.UTF8, bytes);

// Formatting
IOUtil.FormatBytes(1_048_576)      // "1 MB"
IOUtil.GenerateRandomFileName(12)  // e.g. "k7mxqr4bnzp2"

// Directory helpers
IOUtil.CreateDirectoryIfNotExists(@"C:\output\images");
IOUtil.CreateDirectoryIfNotExists(new List<string> { @"C:\a", @"C:\b" });

long size  = IOUtil.CalculateDirectorySize(@"C:\MyFolder");
int  count = IOUtil.GetFileCount(@"C:\Photos", new List<string> { ".jpg", ".png" });
var  files = IOUtil.GetFiles(@"C:\Photos",     new List<string> { ".jpg" });
bool has   = IOUtil.HasImageFiles(@"C:\Photos", new List<string> { "*.jpg", "*.png" });
```

---

### CryptoUtil — Hashing

```csharp
string hash256 = CryptoUtil.CalculateHash_SHA_256("my-password"); // Base64 SHA-256
string hash512 = CryptoUtil.CalculateHash_SHA_512("my-password"); // Base64 SHA-512

byte[] raw = CryptoUtil.CalculateHash_SHA_256(Encoding.UTF8.GetBytes("data"));
```

---

### RandomUtil — Randomisation

```csharp
int    n   = RandomUtil.Next(100);                // 0–99
int    n2  = RandomUtil.Next(10, 50);             // 10–49
string s   = RandomUtil.GetRandomString(16);      // mixed-case + digits, 16 chars
string s2  = RandomUtil.GetRandomString(8, lowercase: true, upperCase: false, numbers: false);
MyEnum val = RandomUtil.RandomEnumValue<MyEnum>();
```

---

### FakeDataUtil — Test Data

Pre-populated name lists for seeding databases, generating fixtures, or populating UI demos.

```csharp
List<string> male    = FakeDataUtil.FirstNames_Male;      // 77 first names
List<string> female  = FakeDataUtil.FirstNames_Female;    // 160 first names
List<string> all     = FakeDataUtil.FirstNames_Combined;  // 237 first names

List<string> english = FakeDataUtil.LastNames_English;    // 100 surnames
List<string> irish   = FakeDataUtil.LastNames_Irish;      // 100 surnames

// Random name
string firstName = FakeDataUtil.FirstNames_Combined.RandomElement();
string lastName  = FakeDataUtil.LastNames_English.RandomElement();
```

---

### GeoUtil — Geography & Coordinates

```csharp
// Decimal degrees or DMS with hemisphere suffix
double lat = GeoUtil.ParseCoordinateString("54° 31' 2.66\" N");  //  54.517...
double lng = GeoUtil.ParseCoordinateString("8.2341 W");           //  -8.2341

// Basic DMS string to decimal
double d = GeoUtil.ParseDmsStringToDecimal("54° 31' 2.66\"");    //  54.517...

// Vector / face-encoding comparison (Euclidean distance)
bool match = GeoUtil.IsEncodingWithinTolerance(encodingA, encodingB, threshold: 0.6f);
```

---

### DirectoryHelper — Path Helpers

```csharp
string rel  = DirectoryHelper.GetPathWithoutDrive(@"C:\Projects\MyApp"); // "Projects\MyApp"
string vol  = DirectoryHelper.GetVolumeName(@"C:\Projects\MyApp");       // "C:\"

// Also accepts DirectoryInfo
string rel2 = DirectoryHelper.GetPathWithoutDrive(new DirectoryInfo(@"D:\data"));
```

---

### DriveUtil — Drive Information *(Windows)*

> Methods decorated with `[SupportedOSPlatform("windows")]` require Windows; cross-platform methods work everywhere.

```csharp
// Cross-platform
bool      removable = DriveUtil.IsDriveRemovable("E");
DriveInfo info      = DriveUtil.GetDriveInfo("C");

// Windows-only: PNP hardware ID for local drives, UNC path for network shares
string id = DriveUtil.GetHardwareIdOrNetworkPath("C"); // e.g. "USBSTOR\DISK&VEN_..."
```

---

## CI / Publishing

Releases are published to [NuGet.org](https://www.nuget.org/packages/IMagic.Utils.Core) automatically via [GitHub Actions](.github/workflows/nuget-publish.yml) when a `v*` tag is pushed:

```bash
git tag v1.0.0.29
git push origin v1.0.0.29
```

You can also trigger a publish manually from the **Actions** tab using the `workflow_dispatch` input with an explicit version number.

The pipeline runs **build → test → pack → push** using the `NUGET_API_KEY` repository secret.

---

## License

[MIT](LICENSE) © Jonathan Mc Namee 2025
