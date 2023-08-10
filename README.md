# AssemblyMetadata.Generators

Source generator to expose assembly attributes as string constants.

[![Build status](https://github.com/loresoft/AssemblyMetadata.Generators/workflows/Build/badge.svg)](https://github.com/loresoft/AssemblyMetadata.Generators/actions)

[![NuGet Version](https://img.shields.io/nuget/v/AssemblyMetadata.Generators.svg?style=flat-square)](https://www.nuget.org/packages/AssemblyMetadata.Generators/)

### Usage

#### Add package

Add the nuget package project to your projects.

`dotnet add package AssemblyMetadata.Generators`

### Generated

This source generator creates an internal partial class called `ThisAssembly` with all the assembly level attributes converted to string constants

```c#
internal static partial class ThisAssembly
{
    public const string TargetFramework = ".NETCoreApp,Version=v7.0";
    public const string Company = "LoreSoft";
    public const string Configuration = "Debug";
    public const string Copyright = "Copyright © 2023 LoreSoft";
    public const string Description = "Source generator to expose assembly attributes as string constants";
    public const string FileVersion = "1.0.0.0";
    public const string InformationalVersion = "1.0.0";
    public const string Product = "AssemblyMetadata.Generators.Tests";
    public const string Title = "AssemblyMetadata.Generators.Tests";
    public const string Version = "1.0.0.0";
    public const string RepositoryUrl = "https://github.com/loresoft/AssemblyMetadata.Generators";
    public const string NeutralResourcesLanguage = "en-US";
}
```
