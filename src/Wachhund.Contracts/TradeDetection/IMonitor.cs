namespace Wachhund.Contracts.TradeDetection;

/// <summary>
/// Monitors the suspicous trading activity
/// </summary>
public interface IMonitor
{
    /// <summary>
    /// Starts the monitoring activity of the suspicious trade deals.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);
}
