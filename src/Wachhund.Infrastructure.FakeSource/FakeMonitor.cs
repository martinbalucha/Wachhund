using Microsoft.Extensions.Logging;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;
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

        int cunter = 0;

        await foreach (var tradeDeal in _fakeDataSource.FetchDataAsync(cancellationToken))
        {
            _ = _detector.DetectAsync(tradeDeal);         
        }
    }
}
