namespace Wachhund.Contracts.TradeDetection;

/// <summary>
/// Represents performed trade deal.
/// </summary>
/// <param name="Id"></param>
/// <param name="Balance"></param>
/// <param name="CurrencyPair"></param>
/// <param name="Lot"></param>
/// <param name="Activity"></param>
public record TradeDeal
{
    public Guid Id { get; init; }
    public string SourceId { get; init; }
    public decimal Balance { get; init; }
    public string CurrencyPair { get; init; }
    public decimal Lot {  get; init; }
    public TradeActivity Activity { get; init; }
    public DateTimeOffset OccurredAt { get; init; }

    public decimal VolumeToBalanceRatio
    {
        get
        {
            if (Balance == 0)
            {
                return 0;
            }

            return Lot / Balance;
        }
    }

    public TradeDeal()
    {
    }

    public TradeDeal(Guid Id,
        string SourceId,
        decimal Balance,
        string CurrencyPair,
        decimal Lot,
        TradeActivity Activity,
        DateTimeOffset OccurredAt)
    {
        this.Id = Id;
        this.Balance = Balance;
        this.CurrencyPair = CurrencyPair;
        this.Lot = Lot;
        this.Activity = Activity;
        this.OccurredAt = OccurredAt;
    }

    public bool IsSuspicious(TradeDeal comparedDeal, decimal suspicousVolumeToBalanceRatio)
    {
        return Math.Abs(VolumeToBalanceRatio - comparedDeal.VolumeToBalanceRatio) <= suspicousVolumeToBalanceRatio;
    }

    public override string ToString()
    {
        return $"{Id}-{SourceId}: Balance {Balance}, {Activity} {CurrencyPair}, {Lot} lot at {OccurredAt.ToString("O")}";
    }
}
