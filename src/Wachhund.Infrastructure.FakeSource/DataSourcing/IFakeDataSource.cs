using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource.DataSourcing;

/// <summary>
/// Represents fake source for trade deals.
/// </summary>
public interface IFakeDataSource
{
    /// <summary>
    /// Asynchronnously fetches fake trade deals.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asynchronnous collection on fake trade deals.</returns>
    IAsyncEnumerable<TradeDeal> FetchDataAsync(CancellationToken cancellationToken);
}
