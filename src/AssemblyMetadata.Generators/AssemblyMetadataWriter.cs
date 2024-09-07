using System.Reflection;

namespace AssemblyMetadata.Generators;

public static class AssemblyMetadataWriter
{
    private static readonly Lazy<string> _informationVersion = new(() =>
    {
        var assembly = typeof(AssemblyMetadataWriter).Assembly;
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return attribute?.InformationalVersion ?? "1.0.0.0";
    });

    public static string Generate(EquatableArray<AssemblyConstant> constants, string? assemblyName = null, string? thisNamespace = null)
    {
        if (constants == null)
            throw new ArgumentNullException(nameof(constants));

        var codeBuilder = new IndentedStringBuilder();
        codeBuilder
            .AppendLine("// <auto-generated />")
            .AppendLine();

        if (!string.IsNullOrEmpty(thisNamespace))
        {
            codeBuilder
                .Append("namespace ")
                .AppendLine(thisNamespace!)
                .AppendLine("{")
                .IncrementIndent();
        }

        codeBuilder
            .AppendLine("/// <summary>")
            .AppendLine("/// Assembly attributes exposed as public constants")
            .AppendLine("/// </summary>");

        codeBuilder
            .Append("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"")
            .Append("AssemblyMetadata.Generators")
            .Append("\", \"")
            .Append(_informationVersion.Value)
            .AppendLine("\")]");

        codeBuilder
            .AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute]")
            .AppendLine("[global::System.Diagnostics.DebuggerStepThroughAttribute]")
            .AppendLine("internal static partial class ThisAssembly")
            .AppendLine("{")
            .IncrementIndent()
            .AppendLine();

        if (!string.IsNullOrEmpty(assemblyName))
        {
            codeBuilder
                .Append("public const string AssemblyName = \"")
                .Append(assemblyName)
                .AppendLine("\";")
                .AppendLine();
        }

        foreach (var constant in constants)
        {
            var name = SafeName(constant.Name);

            codeBuilder
                .Append("public const string ")
                .Append(name)
                .Append(" = ")
                .Append(constant.Value)
                .AppendLine(";")
                .AppendLine();
        }

        codeBuilder
            .DecrementIndent()
            .AppendLine("}"); // class

        if (!string.IsNullOrEmpty(thisNamespace))
        {
            codeBuilder
                .DecrementIndent()
                .AppendLine("}"); // namespace
        }

        return codeBuilder.ToString();
    }

    public static string SafeName(string name)
    {
        return ToPropertyName(name.AsSpan());
    }

    public static string ToPropertyName(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return string.Empty;

        // find the new string size
        var resultSize = 0;
        for (int i = 0; i < span.Length; i++)
        {
            // first char can only be a letter
            if (resultSize == 0 && char.IsLetter(span[i]))
                resultSize++;
            else if (resultSize > 0 && char.IsLetterOrDigit(span[i]))
                resultSize++;
        }

        Span<char> result = stackalloc char[resultSize];

        var written = 0;
        var nextUpper = true;

        for (int read = 0; read < span.Length; read++)
        {
            if ((written == 0 && !char.IsLetter(span[read])) || !char.IsLetterOrDigit(span[read]))
            {
                nextUpper = true;
                continue;
            }

            if (nextUpper)
                result[written++] = char.ToUpper(span[read]);
            else
                result[written++] = span[read];

            nextUpper = false;
        }

        return result.ToString();
    }

}
