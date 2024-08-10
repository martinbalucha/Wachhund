namespace Wachhund.Contracts.TradeDetection;

/// <summary>
/// Represents performed trade deal.
/// </summary>
/// <param name="Id"></param>
/// <param name="Balance"></param>
/// <param name="CurrencyPair"></param>
/// <param name="Lot"></param>
/// <param name="Activity"></param>
public record TradeDeal(Guid Id, Guid SourceId, decimal Balance, string CurrencyPair, decimal Lot, TradeActivity Activity);
