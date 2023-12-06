using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AssemblyMetadata.Generators;

[Generator(LanguageNames.CSharp)]
public class AssemblyMetadataGenerator : IIncrementalGenerator
{
    private static readonly HashSet<string> _attributes = new()
    {
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
    };

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

        IncrementalValuesProvider<EquatableArray<AssemblyConstant>> constants = provider
            .Select(static (item, _) => item.Constants)
            .Where(static item => item.Count > 0);

        context.RegisterSourceOutput(constants, GenerateOutput);
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
                var value = argument.Value?.ToString() ?? string.Empty;

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
                var value = valueArgument.Value?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    continue;

                // prevent duplicates
                if (constants.Any(c => c.Name == key))
                    continue;

                var constant = new AssemblyConstant(key, value);
                constants.Add(constant);
            }
        }

        return new GeneratorContext(Enumerable.Empty<Diagnostic>(), constants);
    }

    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    private void GenerateOutput(SourceProductionContext context, EquatableArray<AssemblyConstant> constants)
    {
        var source = AssemblyMetadataWriter.Generate(constants);

        context.AddSource("AssemblyMetadata.g.cs", source);
    }
}
