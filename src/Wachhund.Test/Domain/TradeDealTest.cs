﻿using Wachhund.Contracts.TradeDetection;
using Xunit;

namespace Wachhund.Test.Domain;

public class TradeDealTest
{
    [Theory]
    [InlineData(100, 1000, 50, 1000, 0.5)]
    [InlineData(100, 1000, 50, 1000, 0.05)]
    [InlineData(100, 1000, 100, 1000, 1)]
    [InlineData(400, 800, 399, 799, 0.01)]
    public void IsSuspicious_VolumeToBalanceRatioBelowTreshhold_ReturnsTrue(decimal lot1, decimal balance1,
        decimal lot2, decimal balance2, decimal treshold)
    {
        // Arrange
        var storedDeal = new TradeDeal
        {
            Activity = TradeActivity.Buy,
            CurrencyPair = "CZKEUR",
            Balance = balance1,
            Lot = lot1,
        };

        var incomingDeal = new TradeDeal
        {
            Activity = TradeActivity.Sell,
            CurrencyPair = storedDeal.CurrencyPair,
            Balance = balance2,
            Lot = lot2,
        };

        // Act
        bool isSuspicous = incomingDeal.IsSuspicious(storedDeal, treshold);

        // Asssert
        Assert.True(isSuspicous);
    }
}
