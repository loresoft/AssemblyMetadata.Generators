using System.Collections.Immutable;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AssemblyMetadata.Generators.Tests;

public class GeneratorTests
{
    [Fact]
    public Task GenerateMicrosoftCSharp()
    {
        var source = @"
using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: TargetFramework("".NETCoreApp,Version=v8.0"", FrameworkDisplayName = "".NET 8.0"")]
[assembly: AssemblyMetadata(""Serviceable"", ""True"")]
[assembly: AssemblyMetadata(""PreferInbox"", ""True"")]
[assembly: AssemblyDefaultAlias(""Microsoft.CSharp"")]
[assembly: NeutralResourcesLanguage(""en-US"")]
[assembly: AssemblyMetadata(""IsTrimmable"", ""True"")]
[assembly: AssemblyCompany(""Microsoft Corporation"")]
[assembly: AssemblyCopyright(""© Microsoft Corporation. All rights reserved."")]
[assembly: AssemblyDescription(""Microsoft.CSharp"")]
[assembly: AssemblyFileVersion(""8.0.23.53103"")]
[assembly: AssemblyInformationalVersion(""8.0.0+5535e31a712343a63f5d7d796cd874e563e5ac14"")]
[assembly: AssemblyProduct(""Microsoft® .NET"")]
[assembly: AssemblyTitle(""Microsoft.CSharp"")]
[assembly: AssemblyMetadata(""RepositoryUrl"", ""https://github.com/dotnet/runtime"")]
[assembly: AssemblyVersion(""8.0.0.0"")]
";

        var (diagnostics, output) = GetGeneratedOutput<AssemblyMetadataGenerator>(source);

        diagnostics.Should().BeEmpty();

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public Task GenerateAssemblyMetadataGeneratorsTests()
    {
        var source = @"
using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: AssemblyMetadata(""HardKey"", ""HardValue"")]
[assembly: AssemblyMetadata(""HardKey"", ""DuplicateValue"")]
[assembly: AssemblyMetadata(""Verify.TargetFrameworks"", """")]
[assembly: AssemblyMetadata(""Verify.ProjectDirectory"", ""D:\\Projects\\GitHub\\AssemblyMetadata.Generators\\test\\AssemblyMetadata.Generators.Tests\\"")]
[assembly: AssemblyMetadata(""Verify.SolutionDirectory"", ""D:\\Projects\\GitHub\\AssemblyMetadata.Generators\\"")]
[assembly: TargetFramework("".NETCoreApp,Version=v8.0"", FrameworkDisplayName = "".NET 8.0"")]
[assembly: AssemblyCompany(""LoreSoft"")]
[assembly: AssemblyConfiguration(""Debug"")]
[assembly: AssemblyCopyright(""Copyright © 2023 LoreSoft"")]
[assembly: AssemblyDescription(""\r\n      Source generator for project and assembly information\r\n      as constants in the global \""AssemblyMetadata\"" \\ class\r\n    "")]
[assembly: AssemblyFileVersion(""1.1.1.0"")]
[assembly: AssemblyInformationalVersion(""1.1.1-alpha.0.1+ca88c967601853f5d364d30e31b8431ffe456289"")]
[assembly: AssemblyProduct(""AssemblyMetadata.Generators.Tests"")]
[assembly: AssemblyTitle(""AssemblyMetadata.Generators.Tests"")]
[assembly: AssemblyMetadata(""RepositoryUrl"", ""https://github.com/loresoft/AssemblyMetadata.Generators"")]
[assembly: NeutralResourcesLanguage(""en-US"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
";

        var (diagnostics, output) = GetGeneratedOutput<AssemblyMetadataGenerator>(source);

        diagnostics.Should().BeEmpty();

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }



    private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HashCode).Assembly.Location),
            });

        var compilation = CSharpCompilation.Create(
            "Test.Generator",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }

}
