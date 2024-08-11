using Bogus;

namespace Wachhund.Infrastructure.FakeSource;

public static class BogusExtensions
{
    /// <summary>
    /// Generates a decimal number with given number of decimals.
    /// </summary>
    /// <param name="randomizer">Randomizer</param>
    /// <param name="min">Lowest possible value</param>
    /// <param name="max">Highest possible value</param>
    /// <param name="decimals">Number of decimal places</param>
    /// <returns>A decimal withing bounds and rounded to desired number of decimals</returns>
    public static decimal Decimal(this Randomizer randomizer, decimal min, decimal max, int decimals)
    {
        decimal number = randomizer.Decimal(min, max);

        return Math.Round(number, decimals);
    }
}
