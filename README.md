# IMagic.Utils.Core

A small collection of general-purpose utility and extension helpers for .NET projects.

This library gathers commonly used helpers (I/O, path helpers, random/fake-data generators, cryptography helpers and a set of extension methods) to reduce boilerplate across applications.

## Features

- Lightweight, framework‑friendly utilities targeting `.NET 9`.
- Helpers grouped under `IMagic.Utils.Core` (see the `IMagic.Utils/Utils` folder).
- Packable as a modern NuGet package via `dotnet pack` / GitHub Actions.

## Installation

Install from NuGet (when published):

```bash
dotnet add package IMagic.Utils.Core --version 1.0.0.27
```

Or build locally and pack:

```bash
dotnet pack IMagic.Utils/IMagic.Utils.csproj -c Release -o ./nupkg
dotnet nuget push ./nupkg/*.nupkg -k <NUGET_API_KEY> -s https://api.nuget.org/v3/index.json
```

## Quick start

- Browse the implementation in the `IMagic.Utils` project to see available helpers under `IMagic.Utils/Utils` and the top-level `ExtensionMethods.cs` file.
- Add the NuGet package to your project and use the types/namespaces exported by the package.

Example (conceptual):

```csharp
// using the library (adjust namespace to match the project)
using IMagic.Utils.Core;

// call utility helpers from the Utils folder or extension methods
// e.g. File/path helpers, crypto helpers, random/fake data generators
```

## CI / Publish

Recommended: publish only from tagged releases. Example GitHub Actions workflow:

```yaml
name: Build / Pack / Publish

on:
  push:
    tags:
      - 'v*'        # publish only on semver tags like v1.0.0

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --configuration Release --no-build

      - name: Pack
        env:
          PACKAGE_VERSION: ${{ github.ref_name }}   # tag name like v1.2.3 (you may strip leading v)
        run: |
          # remove leading 'v' if you tag like 'v1.2.3'
          VER=${PACKAGE_VERSION#v}
          dotnet pack IMagic.Utils/IMagic.Utils.csproj -c Release -o ./nupkg /p:PackageVersion=$VER --no-build

      - name: Publish to NuGet.org
        env:
          NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}
        run: dotnet nuget push ./nupkg/*.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json --skip-duplicate
```

## Contributing

Contributions welcome via PR. Please:

- Target the `main` (or protected) branch via PR.
- Include unit tests for new helpers in the `IMagic.Core.Tests` project.
- Follow the existing coding style and add XML docs where appropriate.

## License

This project uses the MIT license. See `LICENSE` (or update the license as needed).

## Repository

Remote origin: `https://github.com/jonathanmcnamee/IMagic.Core` (update if different)
