using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using Wachhund.Infrastructure.FakeSource.DataSourcing;
using Wachhund.Infrastructure.FakeSource.TradeDealGenerating;
using Xunit;


namespace Wachhund.Test.FakeSource;

public class FakeDataSourceTest
{
    private readonly IFakeTradeDealGenerator _generator;
    private FakeDataSource _dataSource;

    public FakeDataSourceTest()
    {
        var generatorConfiguration = new FakeDataSourceGeneratingConfiguration
        {
            AllowedCurrenciesIso4217 = ["EUR", "USD", "CHF"]
        };

        var generatorOptions = new Mock<IOptions<FakeDataSourceGeneratingConfiguration>>();
        generatorOptions.SetupGet(o => o.Value).Returns(generatorConfiguration);

        _generator = new BogusTradeDealGenerator(generatorOptions.Object);
    }

    [Theory]
    [InlineData(5, 1, 10)]
    [InlineData(5, 50, 10)]
    [InlineData(10, 100, 200)]
    public async Task FetchDataAsync_IncomingData_NumberOfDealsInCorrectRange(int secondsToRun, int minPause, int maxPause)
    {
        // Arrange
        int lowerBrakedDealBound = (secondsToRun * 1000) / maxPause;
        int upperBrakedDealBound = (secondsToRun * 1000) / minPause;

        // Act
        var noPauseDealsTask = RunTestGeneratingAsync(secondsToRun, 0, 0);
        var brakedDealsTask = RunTestGeneratingAsync(secondsToRun, minPause, maxPause);

        int noPauseDealsCount = await noPauseDealsTask;
        int brakedDealsCount = await brakedDealsTask;

        // Assert
        brakedDealsCount.Should().BeLessThan(noPauseDealsCount)
            .And.BeGreaterThanOrEqualTo(lowerBrakedDealBound)
            .And.BeLessThanOrEqualTo(upperBrakedDealBound);
    }

    private async Task<int> RunTestGeneratingAsync(int secondsToRun, int minPause, int maxPause)
    {
        int dealsRecorded = 0;

        var cancellationTokenSource = new CancellationTokenSource();
        var stopwatch = new Stopwatch();

        var configuration = new FakeDataSourceConfiguration()
        {
            MinMillisecondsBetweenDeals = minPause,
            MaxMillisecondsBetweenDeals = maxPause
        };

        var options = new Mock<IOptions<FakeDataSourceConfiguration>>();
        options.SetupGet(c => c.Value).Returns(configuration);

        _dataSource = new FakeDataSource(_generator, options.Object);

        // Act
        stopwatch.Start();

        await foreach (var deal in _dataSource.FetchDataAsync(cancellationTokenSource.Token))
        {
            if (stopwatch.ElapsedMilliseconds >= secondsToRun * 1000)
            {                
                cancellationTokenSource.Cancel();                
                break;
            }

            dealsRecorded++;
        }

        stopwatch.Stop();

        return dealsRecorded;
    }
}
