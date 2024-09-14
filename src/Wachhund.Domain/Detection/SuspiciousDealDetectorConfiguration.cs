namespace Wachhund.Domain.Detection;

public record SuspiciousDealDetectorConfiguration
{
    public int OpenTimeDeltaMilliseconds { get; init; }
    public decimal SuspicousVolumeToBalanceRatio { get; init; }
}
