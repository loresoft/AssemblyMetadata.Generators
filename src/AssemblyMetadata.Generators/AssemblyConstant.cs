namespace AssemblyMetadata.Generators;

public class AssemblyConstant : IEquatable<AssemblyConstant>
{
    public AssemblyConstant(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public string Value { get; }

    public bool Equals(AssemblyConstant other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Name == other.Name
            && Value == other.Value;
    }

    public override bool Equals(object value) => value is AssemblyConstant assemblyContant && Equals(assemblyContant);

    public override int GetHashCode() => HashCode.Combine(Name, Value);

    public static bool operator ==(AssemblyConstant left, AssemblyConstant right) => Equals(left, right);

    public static bool operator !=(AssemblyConstant left, AssemblyConstant right) => !Equals(left, right);

    public override string ToString() => $"Name: {Name}; Value: {Value}";
}
