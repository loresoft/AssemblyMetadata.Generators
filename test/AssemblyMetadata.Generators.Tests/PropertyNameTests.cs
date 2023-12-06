namespace AssemblyMetadata.Generators.Tests;

public class PropertyNameTests
{
    [Theory]
    [InlineData("testName", "TestName")]
    [InlineData("test Name", "TestName")]
    [InlineData("test_Name", "TestName")]
    [InlineData(" test Name", "TestName")]
    [InlineData("123testName", "TestName")]
    [InlineData("%testName", "TestName")]
    [InlineData("Who am I?", "WhoAmI")]
    [InlineData("Hello|Who|Am|I?", "HelloWhoAmI")]
    public void PropertyName(string input, string expected)
    {
        var actual = AssemblyMetadataWriter.ToPropertyName(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Name\\Test", "Name\\\\Test")]
    [InlineData("Name\"Test\"", "Name\\\"Test\\\"")]
    public void SafeValue(string input, string expected)
    {
        var actual = AssemblyMetadataWriter.SafeValue(input);
        Assert.Equal(expected, actual);
    }
}
