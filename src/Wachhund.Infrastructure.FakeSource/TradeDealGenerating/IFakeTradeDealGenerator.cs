using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

public interface IFakeTradeDealGenerator
{
    /// <summary>
    /// Generates a given number of fake trade deals.
    /// </summary>
    /// <param name="count">Desired number of fake trade deals.</param>
    /// <returns></returns>
    IEnumerable<TradeDeal> Generate(int count);
}
