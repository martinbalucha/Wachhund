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
    private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _rwLocks = new();

    public InMemoryTradeDealCache(ILogger<InMemoryTradeDealCache> logger)
    {
        _logger = logger;
    }

    public Task CleanupCacheAsync(DateTimeOffset cutoffDate)
    {
        try
        {
            Parallel.ForEach(_cache, bucket =>
            {
                string currencyPair = bucket.Key;
                _rwLocks.TryGetValue(currencyPair, out var rwLock);

                rwLock?.EnterWriteLock();

                try
                {
                    var oldDeals = bucket.Value.Where(kv => kv.Key.OccurredAt > cutoffDate).ToArray();

                    foreach (var oldDeal in oldDeals)
                    {
                        if (!bucket.Value.TryRemove(oldDeal))
                        {
                            _logger.LogDebug("Could not remove deal with ID='{DealId}'", oldDeal.Key.Id);
                        }

                        _logger.LogInformation($"Removed {oldDeals.Length} for '{bucket.Key}'");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, "An error ocurred for cleanup of CurrencyPair='{CurrencyPair}' bucket", currencyPair);
                    throw;
                }
                finally
                {
                    rwLock?.ExitWriteLock();
                }
            });
        }
        catch (AggregateException ex)
        {
            _logger.LogError(ex, "An error ocurred during the cleanup.");
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<TradeDeal>> GetDealsLaterThenAsync(string currencyPair, DateTimeOffset latestDealsDate)
    {
        var rwLock = _rwLocks.GetOrAdd(currencyPair, k => new ReaderWriterLockSlim());

        rwLock.EnterReadLock();

        try
        {
            if (!_cache.TryGetValue(currencyPair, out var soughtDeals))
            {
                return Task.FromResult(Enumerable.Empty<TradeDeal>());
            }

            return Task.FromResult(soughtDeals.Select(kv => kv.Key)
                                              .Where(d => d.OccurredAt <= latestDealsDate));
        }
        finally 
        { 
            rwLock.ExitReadLock();
        }     
    }

    public Task StoreAsync(TradeDeal tradeDeal)
    {
        var rwLock = _rwLocks.GetOrAdd(tradeDeal.CurrencyPair, k => new ReaderWriterLockSlim());

        try
        {
            rwLock.EnterWriteLock();

            var dealsForCurrency = _cache.GetOrAdd(tradeDeal.CurrencyPair, (k) => new());

            dealsForCurrency.TryAdd(tradeDeal, default);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error ocurred when entering a deal with ID='{Id}' for CurrencyPair={CurrencyPair}", 
                tradeDeal.Id, tradeDeal.CurrencyPair);

            throw;
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }
}
