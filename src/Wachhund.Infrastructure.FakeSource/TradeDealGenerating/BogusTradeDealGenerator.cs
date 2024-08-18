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
    private readonly FakeDataSourceGeneratingConfiguration _fakeDataConfiguration;

    public BogusTradeDealGenerator(IOptions<FakeDataSourceGeneratingConfiguration> fakeDataConfiguration)
    {
        _fakeDataConfiguration = fakeDataConfiguration.Value;

        _tradeDealFaker.RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.SourceId, _fakeDataConfiguration.FakeDataSourceId)
            .RuleFor(t => t.Activity, f => f.PickRandom(new List<TradeActivity> { TradeActivity.Buy, TradeActivity.Sell }))
            .RuleFor(t => t.OccurredAt, f => DateTimeOffset.Now)
            .RuleFor(t => t.CurrencyPair, f => GenerateRandomCurrencyPair())
            .RuleFor(t => t.Balance, f => f.Finance.Random.Decimal(MinimumAccountBalance, MaximumAccountBalance, 2))
            .RuleFor(t => t.Lot, f => f.Finance.Random.Decimal(0.1m, 100000m, 2));
    }

    public TradeDeal Generate()
    {
        return _tradeDealFaker.Generate();
    }

    public IEnumerable<TradeDeal> Generate(int count)
    {
        return _tradeDealFaker.Generate(count);
    }

    public IEnumerable<TradeDeal> GenerateForever()
    {
        return _tradeDealFaker.GenerateForever();
    }

    private string GenerateRandomCurrencyPair()
    {
        string baseCurrency = GenerateRandomCurrency();
        string quoteCurrency = GenerateRandomCurrency();

        while (baseCurrency == quoteCurrency)
        {
            quoteCurrency = GenerateRandomCurrency();
        }

        return baseCurrency + quoteCurrency;
    }

    private string GenerateRandomCurrency()
    {
        int index = random.Next(0, _fakeDataConfiguration.AllowedCurrenciesIso4217.Length);

        return _fakeDataConfiguration.AllowedCurrenciesIso4217.ElementAt(index);
    }
}
