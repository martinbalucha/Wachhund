using Bogus;
using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Test.Domain.Utils;

public class DealsToDetectGenerator
{
    private readonly Faker<TradeDeal> faker = new Faker<TradeDeal>();

    //private IEnumerable<TradeDeal> GenerateSuspiciousDeals(TradeDeal tradeDeal, int suspicousRatio, int timeDeltaMilliseconds, int count)
    //{
    //    faker.RuleFor(t => t.Id, f => Guid.NewGuid())
    //        .RuleFor(t => t.OccurredAt, f => tradeDeal.OccurredAt.AddMicroseconds(f.Random.Int(-timeDeltaMilliseconds, 0)))
    //        .RuleFor(t => t.CurrencyPair, f => tradeDeal.CurrencyPair)
    //        .RuleFor(t => t.);

    //    return faker.Generate(count);
    //}

    private void SetupFaker(string currencyPair, decimal suspicousVolumeToBalanceRatio)
    {
        //faker.RuleFor(t => t.Id, Guid.NewGuid())
        //    .RuleFor(t => t.Balance, )
        //    .RuleFor();
    }
}
