using Infrastructure.Utils.Numbers;
using Xunit;

namespace UnitTestProject;

public class Numbers
{
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(1, 1.49, 1)]
    [InlineData(1, 1.5, 2)]
    [InlineData(0.5, 0.74, 0.5)]
    [InlineData(0.5, 0.75, 1)]
    [InlineData(0.5, 1.24, 1)]
    [InlineData(0.5, 1.25, 1.5)]
    [InlineData(0.5, 1.49, 1.5)]
    [InlineData(0.5, 1.5, 1.5)]
    [InlineData(0.5, 1.51, 1.5)]
    [InlineData(0.5, 1.74, 1.5)]
    [InlineData(0.5, 1.75, 2)]
    public void StepNormalize(double step, double number, double expected)
    {
        var result = number.StepNormalize(step);
        Assert.Equal(expected, result);
    }
}