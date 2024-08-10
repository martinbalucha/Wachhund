using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource;

/// <summary>
/// Implementation of <see cref="IFakeDataSource"/> using Bogus NuGet
/// </summary>
public class BogusDataSource : IFakeDataSource
{
    public IAsyncEnumerable<TradeDeal> FetchDataAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
