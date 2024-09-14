using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Domain.Detection.Caching;
using Xunit;

namespace Wachhund.Test.Domain;

public class InMemoryTradeDealCacheTest
{
    private Mock<ILogger<InMemoryTradeDealCache>> _logger = new();
    private Mock<IOptions<InMemoryCacheConfiguration>> _options = new();

    [Fact]
    public async Task StoreAsync_SingleTradeStored_ValueStored()
    {
        // Arrange
        var configuration = new InMemoryCacheConfiguration
        {
            CleanupIntervalInMilliseconds = 2000
        };

        var tradeDeal = new TradeDeal()
        {
            Id = Guid.NewGuid(),
            Activity = TradeActivity.Buy,
            CurrencyPair = "CZKEUR",
            OccurredAt = DateTime.UtcNow.AddMilliseconds(-50),
        };

        _options.SetupGet(o => o.Value).Returns(configuration);

        var cache = new InMemoryTradeDealCache(_logger.Object, _options.Object);

        // Act
        await cache.StoreAsync(tradeDeal);

        // Assert
        var storedDeals = await cache.GetDealsEarlierThenAsync(tradeDeal.CurrencyPair, DateTimeOffset.UtcNow);

        storedDeals.Should().HaveCount(1).And.ContainEquivalentOf(tradeDeal);
    }

    [Fact]
    public async Task StoreAsync_MultipleDealsStored_ValuesStored()
    {
        // Arrange
        var tradeDeal1 = new TradeDeal()
        {
            Id = Guid.NewGuid(),
            Activity = TradeActivity.Buy,
            CurrencyPair = "CZKEUR",
            OccurredAt = DateTimeOffset.UtcNow.AddMilliseconds(-10)
        };

        var tradeDeal2 = new TradeDeal
        {
            Id = Guid.NewGuid(),
            Activity = TradeActivity.Sell,
            CurrencyPair = "CZKEUR",
            OccurredAt = DateTimeOffset.UtcNow.AddMilliseconds(-9)
        };

        var tradeDeal3 = new TradeDeal
        {
            Id = Guid.NewGuid(),
            Activity = TradeActivity.Sell,
            CurrencyPair = "USDCHF",
            OccurredAt = DateTimeOffset.UtcNow.AddMilliseconds(-5)
        };

        var configuration = new InMemoryCacheConfiguration
        {
            CleanupIntervalInMilliseconds = 2000
        };

        _options.SetupGet(o => o.Value).Returns(configuration);

        var cache = new InMemoryTradeDealCache(_logger.Object, _options.Object);

        // Act
        var tasks = new Task[]
        { 
            cache.StoreAsync(tradeDeal1),
            cache.StoreAsync(tradeDeal2),
            cache.StoreAsync(tradeDeal3)
        };

        await Task.WhenAll(tasks);

        // Assert
        var czkEurDeals = await cache.GetDealsEarlierThenAsync(tradeDeal1.CurrencyPair, DateTimeOffset.UtcNow);
        czkEurDeals.Should().HaveCount(2).And.Contain([tradeDeal1, tradeDeal2]);

        var usdChfDeals = await cache.GetDealsEarlierThenAsync(tradeDeal3.CurrencyPair, DateTimeOffset.UtcNow);
        usdChfDeals.Should().HaveCount(1).And.Contain(tradeDeal3);
    }

    [Fact]
    public async Task GetDealsEarlierThenAsync_NoDealsStored_EmptyCollectionReturned()
    {
        // Arrange
        var configuration = new InMemoryCacheConfiguration
        {
            CleanupIntervalInMilliseconds = 2000
        };

        _options.SetupGet(o => o.Value).Returns(configuration);

        var cache = new InMemoryTradeDealCache(_logger.Object, _options.Object);

        // Act
        var storedDeals = await cache.GetDealsEarlierThenAsync("CZK", DateTimeOffset.UtcNow);

        // Assert
        storedDeals.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDealsEarlierThenAsync_NoDealMatchingCriteria_EmptyCollectionReturned()
    {
        // Arrange
        var configuration = new InMemoryCacheConfiguration
        {
            CleanupIntervalInMilliseconds = 2000
        };

        var tradeDeal = new TradeDeal()
        {
            Id = Guid.NewGuid(),
            Activity = TradeActivity.Buy,
            CurrencyPair = "CZKEUR",
            OccurredAt = DateTime.UtcNow.AddMilliseconds(-50),
        };

        _options.SetupGet(o => o.Value).Returns(configuration);

        var cache = new InMemoryTradeDealCache(_logger.Object, _options.Object);
        await cache.StoreAsync(tradeDeal);

        // Act
        var storedDeals = await cache.GetDealsEarlierThenAsync(tradeDeal.CurrencyPair, DateTimeOffset.UtcNow.AddMilliseconds(-100));

        // Assert

        storedDeals.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDealsEarlierThenAsync_WaitsForCleanup_EmptyCollectionReturned()
    {
        // Arrange
        var configuration = new InMemoryCacheConfiguration
        {
            CleanupIntervalInMilliseconds = 1000
        };

        var tradeDeal = new TradeDeal()
        {
            Id = Guid.NewGuid(),
            Activity = TradeActivity.Buy,
            CurrencyPair = "CZKEUR",
            OccurredAt = DateTime.UtcNow.AddMilliseconds(-50),
        };

        _options.SetupGet(o => o.Value).Returns(configuration);

        var cache = new InMemoryTradeDealCache(_logger.Object, _options.Object);
        await cache.StoreAsync(tradeDeal);

        // Act
        await Task.Delay(configuration.CleanupIntervalInMilliseconds + 50);

        var storedDeals = await cache.GetDealsEarlierThenAsync(tradeDeal.CurrencyPair, DateTimeOffset.UtcNow);

        // Assert
        storedDeals.Should().BeEmpty();
    }
}
