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
    public decimal Balance { get; init; }
    public string CurrencyPair { get; init; }
    public decimal Lot {  get; init; }
    public TradeActivity Activity { get; init; }

    public TradeDeal()
    {
    }

    public TradeDeal(Guid Id, Guid SourceId, decimal Balance, string CurrencyPair, decimal Lot, TradeActivity Activity)
    {
        this.Id = Id;
        this.Balance = Balance;
        this.CurrencyPair = CurrencyPair;
        this.Lot = Lot;
        this.Activity = Activity;
    }
}
