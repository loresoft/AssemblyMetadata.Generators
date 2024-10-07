using System.Collections.Immutable;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AssemblyMetadata.Generators;

[Generator(LanguageNames.CSharp)]
public class AssemblyMetadataGenerator : IIncrementalGenerator
{
    private static readonly HashSet<string> _attributes =
    [
        nameof(AssemblyCompanyAttribute),
        nameof(AssemblyConfigurationAttribute),
        nameof(AssemblyCopyrightAttribute),
        nameof(AssemblyCultureAttribute),
        nameof(AssemblyDelaySignAttribute),
        nameof(AssemblyDescriptionAttribute),
        nameof(AssemblyFileVersionAttribute),
        nameof(AssemblyInformationalVersionAttribute),
        nameof(AssemblyKeyFileAttribute),
        nameof(AssemblyKeyNameAttribute),
        nameof(AssemblyMetadataAttribute),
        nameof(AssemblyProductAttribute),
        nameof(AssemblySignatureKeyAttribute),
        nameof(AssemblyTitleAttribute),
        nameof(AssemblyTrademarkAttribute),
        nameof(AssemblyVersionAttribute),
        nameof(NeutralResourcesLanguageAttribute),
        nameof(TargetFrameworkAttribute),
        "UserSecretsIdAttribute"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "System.Reflection.AssemblyVersionAttribute",
                predicate: SyntacticPredicate,
                transform: SemanticTransform
            )
            .Where(static context => context is not null);

        var diagnostics = provider
            .Select(static (item, _) => item.Diagnostics)
            .Where(static item => item.Count > 0);

        context.RegisterSourceOutput(diagnostics, ReportDiagnostic);

        var constants = provider
            .Select(static (item, _) => item.Constants)
            .Where(static item => item.Count > 0);

        var assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName);

        var globalOptions = context.AnalyzerConfigOptionsProvider
            .Select(static (c, _) =>
            {
                c.GlobalOptions.TryGetValue("build_property.AssemblyName", out var assemblyName);
                c.GlobalOptions.TryGetValue("build_property.DefineConstants", out var defineConstants);
                c.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
                c.GlobalOptions.TryGetValue("build_property.ThisAssemblyNamespace", out var thisNamespace);

                var globalOptions = new GlobalOptions(
                    AssemblyName: assemblyName,
                    DefineConstants: defineConstants,
                    RootNamespace: rootNamespace,
                    ThisAssemblyNamespace: thisNamespace);

                return globalOptions;
            });

        var options = assemblyName.Combine(globalOptions);

        context.RegisterSourceOutput(constants.Combine(options), GenerateOutput);
    }

    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        // only want assembly level attributes
        return syntaxNode is CompilationUnitSyntax;
    }

    private static GeneratorContext SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var attributes = context.TargetSymbol.GetAttributes();
        var constants = new List<AssemblyConstant>();

        foreach (var attribute in attributes)
        {
            var name = attribute.AttributeClass?.Name;
            if (name == null || !_attributes.Contains(name))
                continue;

            if (attribute.ConstructorArguments.Length == 1)
            {
                // remove Assembly
                if (name.Length > 8 && name.StartsWith("Assembly"))
                    name = name.Substring(8);

                // remove Attribute
                if (name.Length > 9)
                    name = name.Substring(0, name.Length - 9);

                var argument = attribute.ConstructorArguments.FirstOrDefault();
                var value = argument.ToCSharpString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var constant = new AssemblyConstant(name, value);
                constants.Add(constant);
            }
            else if (name == nameof(AssemblyMetadataAttribute) && attribute.ConstructorArguments.Length == 2)
            {
                var nameArgument = attribute.ConstructorArguments[0];
                var key = nameArgument.Value?.ToString() ?? string.Empty;

                var valueArgument = attribute.ConstructorArguments[1];
                var value = valueArgument.ToCSharpString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    continue;

                // prevent duplicates
                if (constants.Any(c => c.Name == key))
                    continue;

                var constant = new AssemblyConstant(key, value);
                constants.Add(constant);
            }
        }

        return new GeneratorContext([], constants);
    }

    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    private void GenerateOutput(SourceProductionContext context, (EquatableArray<AssemblyConstant> constants, (string? assemblyName, GlobalOptions globalOptions) options) parameters)
    {

        var constants = new List<AssemblyConstant>(parameters.constants);

        if (!constants.Any(p => p.Name == nameof(GlobalOptions.AssemblyName)))
        {
            var assemblyName = parameters.options.globalOptions.AssemblyName ?? parameters.options.assemblyName ?? string.Empty;
            constants.Add(new AssemblyConstant(nameof(GlobalOptions.AssemblyName), $"\"{assemblyName}\""));
        }

        if (!constants.Any(p => p.Name == nameof(GlobalOptions.DefineConstants)))
        {
            var defineConstants = parameters.options.globalOptions.DefineConstants;
            if (!string.IsNullOrEmpty(defineConstants))
                constants.Add(new AssemblyConstant(nameof(GlobalOptions.DefineConstants), $"\"{defineConstants}\""!));
        }

        if (!constants.Any(p => p.Name == nameof(GlobalOptions.RootNamespace)))
        {
            var rootNamespace = parameters.options.globalOptions.RootNamespace;
            if (!string.IsNullOrEmpty(rootNamespace))
                constants.Add(new AssemblyConstant(nameof(GlobalOptions.RootNamespace), $"\"{rootNamespace}\""!));
        }

        var source = AssemblyMetadataWriter.Generate(constants, parameters.options.globalOptions.ThisAssemblyNamespace);

        context.AddSource("AssemblyMetadata.g.cs", source);
    }
}
