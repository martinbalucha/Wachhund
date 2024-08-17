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

        while (!cancellationToken.IsCancellationRequested)
        {
            var fakeDeals = _generator.GenerateForever();

            foreach (var deal in fakeDeals)
            {
                yield return deal;

                int pause = random.Next(_configuration.MinMillisecondsBetweenDeals, _configuration.MaxMillisecondsBetweenDeals);

                await Task.Delay(pause, cancellationToken);
            }
        }
    }
}
