namespace Wachhund.Infrastructure.FakeSource.DataSourcing;

public record FakeDataSourceConfiguration
{
    public int MinMillisecondsBetweenDeals { get; init; }
    public int MaxMillisecondsBetweenDeals { get; init; }
}
