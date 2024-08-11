using Microsoft.Extensions.Options;
using Moq;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Infrastructure.FakeSource.TradeDealGenerating;
using Xunit;

namespace Wachhund.Test.FakeSource;

public class BogusTradeDealGeneratorTest
{
    private readonly FakeDataSourceGeneratingConfiguration _configuration;
    private readonly BogusTradeDealGenerator _generator;

    public BogusTradeDealGeneratorTest()
    {
        _configuration = new FakeDataSourceGeneratingConfiguration
        {
            AllowedCurrenciesIso4217 = [ "EUR", "USD", "CHF" ]
        };

        var optionsMock = new Mock<IOptions<FakeDataSourceGeneratingConfiguration>>();
        optionsMock.SetupGet(x => x.Value).Returns(_configuration);

        _generator = new BogusTradeDealGenerator(optionsMock.Object);
    }

    [Fact]
    public void Generate_OneTradeDeal_SatisfiesCriteria()
    {
        // Act
        var result = _generator.Generate(1).ToList();

        // Assert
        Assert.Single(result);

        var tradeDeal = result.ElementAt(0);

        AssertTradeDealValidity(tradeDeal);
    }

    [Fact]
    public void Generate_MultipleDeals_SatisfiesCriteria()
    {
        // Arrange
        const int dealCount = 15;

        // Act
        var result = _generator.Generate(dealCount).ToList();

        // Assert
        Assert.Equal(dealCount, result.Count());

        foreach (var deal in result )
        {
            AssertTradeDealValidity(deal);
        }
    }

    private void AssertTradeDealValidity(TradeDeal tradeDeal)
    {
        Assert.NotEqual(default, tradeDeal.Id);
        Assert.True(AreCurrenciesDifferent(tradeDeal.CurrencyPair));
        Assert.NotEqual(0, tradeDeal.Balance);
        Assert.NotEqual(0, tradeDeal.Lot);
        Assert.NotEqual(TradeActivity.None, tradeDeal.Activity);
    }

    private bool AreCurrenciesDifferent(string currencyPair)
    {
        string firstCurrency = currencyPair.Substring(0, 3);
        string secondCurrency = currencyPair.Substring(3, 3);

        return !firstCurrency.Equals(secondCurrency, StringComparison.OrdinalIgnoreCase)
            && _configuration.AllowedCurrenciesIso4217.Contains(firstCurrency)
            && _configuration.AllowedCurrenciesIso4217.Contains(secondCurrency);
    }
}
