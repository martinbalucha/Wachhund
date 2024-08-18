namespace Wachhund.Contracts.TradeDetection;

public interface ISuspiciousDealDetector
{
    /// <summary>
    /// Detects whether the incoming deal is suspicious
    /// </summary>
    /// <param name="incomingDeal">Incoming deal that is to be processed</param>
    /// <returns></returns>
    Task<IEnumerable<TradeDeal>> DetectAsync(TradeDeal incomingDeal);
}
