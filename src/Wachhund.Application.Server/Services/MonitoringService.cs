
using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Application.Server.Services;

/// <summary>
/// Starts the monitoring process across all configured sources.
/// </summary>
public class MonitoringService : BackgroundService
{
    private readonly IEnumerable<IMonitor> monitors;

    public MonitoringService(IEnumerable<IMonitor> monitors)
    {
        this.monitors = monitors;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = monitors.Select(m => m.StartAsync(stoppingToken));

        return Task.WhenAll(tasks);
    }
}
