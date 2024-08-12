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
    private readonly FakeDataSourceConfiguration _configuration;
    private readonly FakeDataSource _dataSource;

    public FakeDataSourceTest()
    {
        _configuration = new FakeDataSourceConfiguration()
        {
            MinDealsPerSecond = 10,
            MaxDealsPerSecond = 100
        };

        var options = new Mock<IOptions<FakeDataSourceConfiguration>>();
        options.SetupGet(c => c.Value).Returns(_configuration);

        var generatorConfiguration = new FakeDataSourceGeneratingConfiguration
        {
            AllowedCurrenciesIso4217 = ["EUR", "USD", "CHF"]
        };

        var generatorOptions = new Mock<IOptions<FakeDataSourceGeneratingConfiguration>>();
        generatorOptions.SetupGet(o => o.Value).Returns(generatorConfiguration);

        _generator = new BogusTradeDealGenerator(generatorOptions.Object);

        _dataSource = new FakeDataSource(_generator, options.Object);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task FetchDataAsync_IncomingData_NumberOfDealsInCorrectRange(int secondsToRun)
    {
        // Arrange
        int dealsRecorded = 0;

        var cancellationTokenSource = new CancellationTokenSource();
        var stopwatch = new Stopwatch();

        int minimumExpectedDeals = secondsToRun * _configuration.MinDealsPerSecond;
        int maximumExpectedDeals = secondsToRun * _configuration.MaxDealsPerSecond;

        // Act
        stopwatch.Start();

        await foreach (var deal in _dataSource.FetchDataAsync(cancellationTokenSource.Token))
        {
            if (stopwatch.ElapsedMilliseconds > secondsToRun * 1000)
            {
                stopwatch.Stop();
                cancellationTokenSource.Cancel();
                break;
            }

            dealsRecorded++;
        }

        // Assert
        Assert.True(minimumExpectedDeals <= dealsRecorded && dealsRecorded <= maximumExpectedDeals);
    }
}
