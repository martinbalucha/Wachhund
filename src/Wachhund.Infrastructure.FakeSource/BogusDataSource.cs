using Bogus;
using Microsoft.Extensions.Options;
using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource;

/// <summary>
/// Implementation of <see cref="IFakeDataSource"/> using Bogus NuGet
/// </summary>
public class BogusDataSource
{
    private readonly Faker<TradeDeal> _tradeDealFaker = new();
    private readonly Random random = new Random();
    private readonly FakeDataSourceConfiguration _fakeDataConfiguration;

    public BogusDataSource(IOptions<FakeDataSourceConfiguration> fakeDataConfiguration)
    {
        _fakeDataConfiguration = fakeDataConfiguration.Value;

        _tradeDealFaker.RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.Activity, f => f.PickRandom<TradeActivity>())
            .RuleFor(t => t.CurrencyPair, f => GenerateRandomCurrency())
            //.RuleFor(t => t.Balance, f => )4
            ;
    }

    private string GenerateRandomCurrencyPair()
    {
        string firstCurrency = GenerateRandomCurrency();
        string secondCurrency = firstCurrency;

        while (firstCurrency == secondCurrency)
        {
            secondCurrency = GenerateRandomCurrency();
        }

        return firstCurrency + secondCurrency;
    }

    private string GenerateRandomCurrency()
    {
        int index = random.Next(0, _fakeDataConfiguration.AllowedCurrenciesIso4217.Length);

        return _fakeDataConfiguration.AllowedCurrenciesIso4217.ElementAt(index);
    }
}
