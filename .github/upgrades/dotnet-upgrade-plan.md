# .NET 9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade IMagic.Utils\IMagic.Utils.csproj
4. Upgrade IMagic.Core.Tests\IMagic.Core.Tests.csproj


## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|


### Aggregate NuGet packages modifications across all projects

No NuGet package updates were discovered by analysis that must be applied as part of the framework upgrade.


### Project upgrade details

#### IMagic.Utils\IMagic.Utils.csproj modifications

Project properties changes:
  - Project file must be converted from legacy (non-SDK) style to SDK-style project format.
  - Target framework should be changed from `net48` (TargetFrameworkVersion `v4.8`) to `net9.0`.

NuGet packages changes:
  - No package version changes required per analysis.

Feature upgrades:
  - Convert project to SDK-style: remove legacy MSBuild Scc properties and update imports to SDK implicit imports.

Other changes:
  - Remove legacy TFS bindings (SccProjectName, SccLocalPath, SccAuxPath, SccProvider) from project file.
  - Ensure any NuGet package references are migrated to PackageReference if desired.


#### IMagic.Core.Tests\IMagic.Core.Tests.csproj modifications

Project properties changes:
  - Project file must be converted from legacy (non-SDK) style to SDK-style project format.
  - Target framework should be changed from `net48` (TargetFrameworkVersion `v4.8`) to `net9.0`.

NuGet packages changes:
  - Update test framework packages to versions compatible with .NET 9.0 (migrate older MSTest packages to latest `MSTest.TestFramework` and `MSTest.TestAdapter` as PackageReference in SDK-style project).

Feature upgrades:
  - Update test project to use SDK-style test project with `IsTestProject` metadata if required and use `dotnet test` compatible packages.

Other changes:
  - Remove legacy TFS bindings (SccProjectName, SccLocalPath, SccAuxPath, SccProvider) from project file.


