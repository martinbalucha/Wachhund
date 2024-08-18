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
    [InlineData(5, 10, 50)]
    [InlineData(10, 100, 200)]
    public async Task FetchDataAsync_IncomingData_NumberOfDealsInCorrectRange(int secondsToRun, int minPause, int maxPause)
    {
        //TODO: rethink this one; it is failing for small delays

        // Arrange
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

        int lowerBrakedDealBound = (secondsToRun * 1000) / maxPause;
        int upperBrakedDealBound = (secondsToRun * 1000) / minPause;

        // Act    
        var fakeDeals = _dataSource.FetchDataAsync(cancellationTokenSource.Token);

        stopwatch.Start();

        await foreach (var deal in fakeDeals)
        {
            if (stopwatch.ElapsedMilliseconds >= secondsToRun * 1000)
            {
                cancellationTokenSource.Cancel();
                break;
            }

            dealsRecorded++;
        }

        stopwatch.Stop();

        // Assert
        dealsRecorded.Should().BeGreaterThanOrEqualTo(lowerBrakedDealBound)
                     .And.BeLessThanOrEqualTo(upperBrakedDealBound);
    }
}
