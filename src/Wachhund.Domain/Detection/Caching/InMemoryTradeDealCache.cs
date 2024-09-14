using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;

namespace Wachhund.Domain.Detection.Caching;

public class InMemoryTradeDealCache : ITradeDealCache
{
    private readonly ILogger<InMemoryTradeDealCache> _logger;
    private readonly InMemoryCacheConfiguration _config;

    /// <summary>
    /// An in-memory cache which separates the deals into "buckets" by the currency pair.
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<TradeDeal, byte>> _cache = new();
    private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _rwLocks = new();

    public InMemoryTradeDealCache(ILogger<InMemoryTradeDealCache> logger,
        IOptions<InMemoryCacheConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;

        _ = CleanupPeriodicallyAsync();
    }

    public Task<IEnumerable<TradeDeal>> GetDealsEarlierThenAsync(string currencyPair, DateTimeOffset latestDealsDate)
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

    private async Task CleanupPeriodicallyAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_config.CleanupIntervalInMilliseconds));

        try
        {
            while (await timer.WaitForNextTickAsync())
            {
                DateTimeOffset cutOffDate = DateTimeOffset.UtcNow.AddMilliseconds(_config.CleanupIntervalInMilliseconds);

                CleanupCache(cutOffDate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the cache cleanup.");
        }
    }

    private void CleanupCache(DateTimeOffset cutoffDate)
    {
        try
        {
            _logger.LogDebug($"Starts cleaning, TID={Thread.CurrentThread.ManagedThreadId}");

            Parallel.ForEach(_cache, bucket =>
            {
                string currencyPair = bucket.Key;

                _rwLocks.TryGetValue(currencyPair, out var rwLock);

                rwLock?.EnterWriteLock();

                try
                {
                    var oldDeals = bucket.Value.Where(kv => kv.Key.OccurredAt < cutoffDate).ToArray();

                    foreach (var oldDeal in oldDeals)
                    {
                        if (!bucket.Value.TryRemove(oldDeal))
                        {
                            _logger.LogWarning("Could not remove deal with ID='{DealId}'", oldDeal.Key.Id);
                            continue;
                        }
                    }

                    _logger.LogInformation($"Removed {oldDeals.Length} for '{bucket.Key}', TID={Thread.CurrentThread.ManagedThreadId}");
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
    }
}
