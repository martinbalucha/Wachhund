
using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Application.Server.Services;

/// <summary>
/// Starts the monitoring process across all configured sources.
/// </summary>
public class MonitoringService : BackgroundService
{
    private readonly IEnumerable<IMonitor> monitors;
    private readonly ILogger<MonitoringService> logger;

    public MonitoringService(IEnumerable<IMonitor> monitors, ILogger<MonitoringService> logger)
    {
        this.monitors = monitors;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting the monitoring process.");

        var tasks = monitors.Select(m => m.StartAsync(stoppingToken));

        await Task.WhenAll(tasks);

        logger.LogInformation("Monitoring process ended.");
    }
}
