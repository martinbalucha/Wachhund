using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;
using Wachhund.Domain;
using Xunit;

namespace Wachhund.Test.Domain;

public class SuspiciousDealDetectorTest
{
    private readonly Mock<ITradeDealCache> _cache;
    private readonly Mock<IOptions<SuspiciousDealDetectorConfiguration>> _options;
    private readonly Mock<ILogger<SuspicousDealDetector>> _logger;

    public SuspiciousDealDetectorTest()
    {
        _cache = new Mock<ITradeDealCache>();
        _options = new Mock<IOptions<SuspiciousDealDetectorConfiguration>>();
        _logger = new Mock<ILogger<SuspicousDealDetector>>();
    }

    [Fact]
    public async Task DetectAsync_EmptyCache_NoSuspicousDealsFound()
    {
        // Arrange
        var occurredAt = DateTimeOffset.Parse("2024-08-19T20:12:31");
        var incomingDeal = new TradeDeal(Guid.NewGuid(), "Test", 10000, "CZKCHF", 10, TradeActivity.Buy, occurredAt);

        var config = new SuspiciousDealDetectorConfiguration
        {
            OpenTimeDeltaMilliseconds = 1000,
            SuspicousVolumeToBalanceRatio = 0.05m
        };

        _options.SetupGet(o => o.Value).Returns(config);

        _cache.Setup(c => c.GetDealsLaterThenAsync(incomingDeal.CurrencyPair, occurredAt.AddMicroseconds(config.OpenTimeDeltaMilliseconds)))
              .ReturnsAsync(new[] { incomingDeal });

        var detector = new SuspicousDealDetector(_cache.Object, _options.Object, _logger.Object);

        // Act
        var suspicousDeals = await detector.DetectAsync(incomingDeal);

        // Assert
        suspicousDeals.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_ContainsExpiredDeals_NoSuspicousDealsFound()
    {
        // Arrange
        var occurredAt = DateTimeOffset.Parse("2024-08-19T20:12:31");
        var incomingDeal = new TradeDeal(Guid.NewGuid(), "Test", 10000, "CZKCHF", 10, TradeActivity.Buy, occurredAt);

        var config = new SuspiciousDealDetectorConfiguration
        {
            OpenTimeDeltaMilliseconds = 1000,
            SuspicousVolumeToBalanceRatio = 0.05m
        };

        _options.SetupGet(o => o.Value).Returns(config);

        var expiredDeal1 = new TradeDeal
        {
            CurrencyPair = incomingDeal.CurrencyPair,
            OccurredAt = occurredAt.AddMicroseconds(2 * config.OpenTimeDeltaMilliseconds),
        };

        var expiredDeal2 = new TradeDeal
        {
            CurrencyPair = incomingDeal.CurrencyPair,
            OccurredAt = occurredAt.AddMicroseconds(config.OpenTimeDeltaMilliseconds + 1)
        };

        _cache.Setup(c => c.GetDealsLaterThenAsync(incomingDeal.CurrencyPair, occurredAt.AddMicroseconds(config.OpenTimeDeltaMilliseconds)))
              .ReturnsAsync(new[] { expiredDeal1, expiredDeal2 });

        var detector = new SuspicousDealDetector(_cache.Object, _options.Object, _logger.Object);

        // Act
        var suspicousDeals = await detector.DetectAsync(incomingDeal);

        // Assert
        suspicousDeals.Should().BeEmpty();
    }
}
