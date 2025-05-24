using Xunit;
using Shared.Tools;
namespace UnitTests.Shared.Tools;
public class Generator
{
    RandomGenerator generator = new();
    [Fact]
    public void GenerateString_NotEmpty_CorrectLenght()
    {
        int lenght = 10;
        string randomString = generator.NextString(lenght);

        Assert.NotEmpty(randomString);
        Assert.Equal(randomString.Length, lenght);
    }

    [Fact]
    public void GenerateInt_NotZero_BetweenMinMax()
    {
        int min = 5;
        int max = 10;
        int randomInt = generator.NextInt(min, max);

        Assert.NotEqual(0, randomInt);
        Assert.True(randomInt >= min);
        Assert.True(randomInt <= max);
    }
}