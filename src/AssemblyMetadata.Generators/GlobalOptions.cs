namespace AssemblyMetadata.Generators;

public readonly record struct GlobalOptions(
    string? AssemblyName,
    string? DefineConstants,
    string? RootNamespace,
    string? ThisAssemblyNamespace,
    string? PackageVersion,
    string? PackageId,
    string? RepositoryBranch,
    string? RepositoryCommit
);
