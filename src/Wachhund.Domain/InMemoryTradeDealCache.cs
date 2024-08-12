using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;

namespace Wachhund.Domain;

public class InMemoryTradeDealCache : ITradeDealCache
{
    /// <summary>
    /// An in-memory cache which separates the deals into "buckets" by the currency pair.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentBag<TradeDeal>> _cache = new();

    public Task CleanupCacheAsync(DateTimeOffset cutoffDate)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TradeDeal>> GetDealsLaterThenAsync(string currencyPair, DateTimeOffset latestDealsDate)
    {
        throw new NotImplementedException();
    }

    public Task StoreAsync(TradeDeal tradeDeal)
    {
        var dealsForCurrency = _cache.GetOrAdd(tradeDeal.CurrencyPair, (k) => new ());

        dealsForCurrency.Add(tradeDeal);

        return Task.CompletedTask;
    }
}
