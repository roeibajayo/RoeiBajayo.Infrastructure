using System;

namespace Infrastructure.Utils.Numbers;

public static class NumbersExtensions
{
    public static double Normalize(this double value, double max) => Normalize(value, 0, max);
    public static double Normalize(this double value, double min, double max)
    {
        if (min >= max)
            throw new ArgumentException("Min must be less than max", nameof(min));

        if (value < min)
            return 0;

        if (value > max)
            return 1;

        return (value - min) / (max - min);
    }
    public static int GetStep(this double value, double stepLength)
    {
        if (stepLength <= 0)
            throw new ArgumentException("Step length must be greater than 0", nameof(stepLength));

        var includedSteps = (int)(Math.Abs(value) / stepLength);
        return includedSteps;
    }
    public static double StepNormalize(this double value, double step)
    {
        if (step <= 0)
            throw new ArgumentException("Step must be greater than 0", nameof(step));

        if (value < 0)
            return -StepNormalize(Math.Abs(value), step);

        var includedSteps = (int)(value / step);
        var rest = value % step;
        if (rest == 0)
            return value;

        var halfStep = step / 2;
        if (rest < halfStep)
            return includedSteps * step;

        return (includedSteps + 1) * step;
    }
}
