using Microsoft.Extensions.Logging;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;
using Wachhund.Infrastructure.FakeSource.DataSourcing;

namespace Wachhund.Infrastructure.FakeSource;

public class FakeMonitor : IMonitor
{
    private readonly IFakeDataSource _fakeDataSource;
    private readonly ITradeDealCache _cache;
    private readonly ILogger<FakeMonitor> _logger;

    public FakeMonitor(IFakeDataSource fakeDataSource,
        ITradeDealCache cache,
        ILogger<FakeMonitor> logger)
    {
        _fakeDataSource = fakeDataSource;
        _cache = cache;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting monitoring process for fake data source.");

        int cunter = 0;

        await foreach (var tradeDeal in _fakeDataSource.FetchDataAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _cache.StoreAsync(tradeDeal);
            
            if (cunter >= 100)
            {
                var removalDate = DateTimeOffset.Now.AddSeconds(-5);
                await _cache.CleanupCacheAsync(removalDate);
            }

            cunter++;
        }
    }
}
