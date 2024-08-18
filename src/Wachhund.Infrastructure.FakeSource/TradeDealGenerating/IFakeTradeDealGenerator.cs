using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

public interface IFakeTradeDealGenerator
{
    /// <summary>
    /// Generates a single fake trade deal.
    /// </summary>
    /// <returns></returns>
    TradeDeal Generate();

    /// <summary>
    /// Generates a given number of fake trade deals.
    /// </summary>
    /// <param name="count">Desired number of fake trade deals.</param>
    /// <returns></returns>
    IEnumerable<TradeDeal> Generate(int count);

    /// <summary>
    /// Gemerates a never-ending sequence of fake trade deals.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TradeDeal> GenerateForever();
}
