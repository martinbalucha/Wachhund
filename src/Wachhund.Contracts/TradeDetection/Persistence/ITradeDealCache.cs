namespace Wachhund.Contracts.TradeDetection.Persistence;

public interface ITradeDealCache
{
    Task StoreAsync(TradeDeal tradeDeal);
}
