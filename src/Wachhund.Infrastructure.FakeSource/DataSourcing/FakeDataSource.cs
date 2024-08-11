using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

namespace Wachhund.Infrastructure.FakeSource.DataSourcing;

public class FakeDataSource : IFakeDataSource
{
    private const int MillisecondsOfWaiting = 1000;

    private readonly IFakeTradeDealGenerator _generator;
    private readonly FakeDataSourceConfiguration _configuration;

    public FakeDataSource(IFakeTradeDealGenerator generator, 
        IOptions<FakeDataSourceConfiguration> configuration)
    {
        _generator = generator;
        _configuration = configuration.Value;
    }

    public async IAsyncEnumerable<TradeDeal> FetchDataAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var random = new Random();
        var stopwatch = new Stopwatch();

        while (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Restart();

            int dealCount = random.Next(_configuration.MinDealsPerSecond, _configuration.MaxDealsPerSecond);

            var fakeDeals = _generator.Generate(dealCount);

            foreach (var deal in fakeDeals)
            {
                yield return deal;
            }

            stopwatch.Stop();

            long remainingMillisecondsToWait = MillisecondsOfWaiting - stopwatch.ElapsedMilliseconds;

            if (remainingMillisecondsToWait > 0)
            {
                // If it is greater than 0, it is in range (0, 1000)
                await Task.Delay((int)remainingMillisecondsToWait, cancellationToken);
            }
        }
    }
}
