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
        var actual = ToPropertyName(input);
        Assert.Equal(expected, actual);
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
