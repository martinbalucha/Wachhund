namespace Wachhund.Domain.Detection.Caching;

public record InMemoryCacheConfiguration
{
    public int CleanupIntervalInMilliseconds { get; init; }
}
