namespace AssemblyMetadata.Generators.Tests;

public class ThisAssemblyTests
{
    [Fact]
    public void ProductAttributeTest()
    {
        Assert.Equal("AssemblyMetadata.Generators.Tests", ThisAssembly.Product);
    }

    [Fact]
    public void CompanyAttributeTest()
    {
        Assert.Equal("LoreSoft", ThisAssembly.Company);
    }

}
