namespace Wachhund.Infrastructure.FakeSource.DataSourcing;

public record FakeDataSourceConfiguration
{
    public int MinDealsPerSecond { get; init; }
    public int MaxDealsPerSecond { get; init; }
}
