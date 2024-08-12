using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;

namespace Wachhund.Domain;

public class InMemoryTradeDealCache : ITradeDealCache
{
    private readonly ILogger<InMemoryTradeDealCache> _logger;

    /// <summary>
    /// An in-memory cache which separates the deals into "buckets" by the currency pair.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<TradeDeal, byte>> _cache = new();

    public InMemoryTradeDealCache(ILogger<InMemoryTradeDealCache> logger)
    {
        _logger = logger;
    }

    public Task CleanupCacheAsync(DateTimeOffset cutoffDate)
    {
        Parallel.ForEach(_cache, bucket =>
        {
            var oldDeals = bucket.Value.Where(kv => kv.Key.OccurredAt > cutoffDate);
            
            foreach (var oldDeal in oldDeals)
            {
                if (!bucket.Value.TryRemove(oldDeal))
                {
                    _logger.LogDebug("Could not remove deal with ID='{DealId}'", oldDeal.Key.Id);
                }
            }
        });

        return Task.CompletedTask;
    }

    public Task<IEnumerable<TradeDeal>> GetDealsLaterThenAsync(string currencyPair, DateTimeOffset latestDealsDate)
    {
        if (!_cache.TryGetValue(currencyPair, out var soughtDeals))
        {
            return Task.FromResult(Enumerable.Empty<TradeDeal>());
        }

        return Task.FromResult(soughtDeals.Select(kv => kv.Key)
                                          .Where(d => d.OccurredAt <= latestDealsDate));
    }

    public Task StoreAsync(TradeDeal tradeDeal)
    {
        var dealsForCurrency = _cache.GetOrAdd(tradeDeal.CurrencyPair, (k) => new ());

        dealsForCurrency.TryAdd(tradeDeal, default);

        return Task.CompletedTask;
    }
}
