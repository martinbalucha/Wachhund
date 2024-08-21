using Microsoft.Extensions.Logging;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Infrastructure.FakeSource.DataSourcing;

namespace Wachhund.Infrastructure.FakeSource;

public class FakeMonitor : IMonitor
{
    private readonly IFakeDataSource _fakeDataSource;
    private readonly ISuspiciousDealDetector _detector;
    private readonly ILogger<FakeMonitor> _logger;

    public FakeMonitor(IFakeDataSource fakeDataSource,
        ISuspiciousDealDetector detector,
        ILogger<FakeMonitor> logger)
    {
        _fakeDataSource = fakeDataSource;
        _detector = detector;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting monitoring process for fake data source.");

        await foreach (var tradeDeal in _fakeDataSource.FetchDataAsync(cancellationToken))
        {
            var suspiciousDeals = await _detector.DetectAsync(tradeDeal);
            
            if (suspiciousDeals.Count() > 0)
            {
                _logger.LogInformation("Suspicious deals found for {DealId}, {CurrencyPair}", tradeDeal.Id, tradeDeal.CurrencyPair);
            }
        }
    }
}
