using Bogus;
using Microsoft.Extensions.Options;
using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

/// <summary>
/// Implementation of <see cref="IFakeTradeDealGenerator"/> using Bogus NuGet
/// </summary>
public class BogusTradeDealGenerator : IFakeTradeDealGenerator
{
    private const decimal MinimumAccountBalance = 1;
    private const decimal MaximumAccountBalance = 10000000;

    private readonly Faker<TradeDeal> _tradeDealFaker = new();
    private readonly Random random = new Random();
    private readonly FakeDataSourceConfiguration _fakeDataConfiguration;

    public BogusTradeDealGenerator(IOptions<FakeDataSourceConfiguration> fakeDataConfiguration)
    {
        _fakeDataConfiguration = fakeDataConfiguration.Value;

        _tradeDealFaker.RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.Activity, f => f.PickRandom<TradeActivity>())
            .RuleFor(t => t.CurrencyPair, f => GenerateRandomCurrencyPair())
            .RuleFor(t => t.Balance, f => f.Finance.Random.Decimal(MinimumAccountBalance, MaximumAccountBalance))
            .RuleFor(t => t.Lot, f => f.Finance.Random.Decimal(0.1m, 100000m));
    }

    public IEnumerable<TradeDeal> Generate(int count)
    {
        return _tradeDealFaker.Generate(count);
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
