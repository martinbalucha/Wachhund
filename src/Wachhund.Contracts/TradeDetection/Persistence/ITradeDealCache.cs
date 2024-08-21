namespace Wachhund.Contracts.TradeDetection.Persistence;

public interface ITradeDealCache
{
    /// <summary>
    /// Stores a new trade deal into the cache.
    /// </summary>
    /// <param name="tradeDeal"></param>
    /// <returns></returns>
    Task StoreAsync(TradeDeal tradeDeal);

    /// <summary>
    /// Retrieves all deals with the given <paramref name="currencyPair"/> that were made
    /// no later than <paramref name="latestDealsDate"/>
    /// </summary>
    /// <param name="currencyPair">A pair of currencies for which trade deals were done</param>
    /// <param name="latestDealsDate">A date after which trade deals were made</param>
    /// <returns>A collection of trade deals satisfying the criteria. Empty collection if none were found.</returns>
    Task<IEnumerable<TradeDeal>> GetDealsLaterThenAsync(string currencyPair, DateTimeOffset latestDealsDate);
}
