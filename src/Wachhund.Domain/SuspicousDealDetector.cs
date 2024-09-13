using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;

namespace Wachhund.Domain;

public class SuspicousDealDetector : ISuspiciousDealDetector
{
    private readonly ITradeDealCache _cache;
    private readonly SuspiciousDealDetectorConfiguration _config;
    private readonly ILogger<SuspicousDealDetector> _logger;

    public SuspicousDealDetector(ITradeDealCache cache, 
        IOptions<SuspiciousDealDetectorConfiguration> config,
        ILogger<SuspicousDealDetector> logger)
    {
        _cache = cache;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<TradeDeal>> DetectAsync(TradeDeal incomingDeal)
    {
        if (incomingDeal is null)
        {
            return Enumerable.Empty<TradeDeal>();
        }

        // Need to store this ASAP so that it can be used by next incoming trade
        await _cache.StoreAsync(incomingDeal);

        var cutOffDate = incomingDeal.OccurredAt.AddMilliseconds(_config.OpenTimeDeltaMilliseconds);

        var relevantDeals = await _cache.GetDealsEarlierThenAsync(incomingDeal.CurrencyPair, cutOffDate);

        var suspiciousDeals = GetSuspicousDeals(incomingDeal, relevantDeals);

        if (suspiciousDeals.Count > 0)
        {
            _logger.LogInformation("Suspicious deals found for {DealId}, {CurrencyPair}", incomingDeal.Id, incomingDeal.CurrencyPair);
        }

        return suspiciousDeals;
    }

    private List<TradeDeal> GetSuspicousDeals(TradeDeal incomingDeal, IEnumerable<TradeDeal> relevantDeals)
    {
        return relevantDeals.Where(d => d.Id != incomingDeal.Id && incomingDeal.IsSuspicious(d, _config.SuspicousVolumeToBalanceRatio))                                        
                            .ToList();
    }
}
