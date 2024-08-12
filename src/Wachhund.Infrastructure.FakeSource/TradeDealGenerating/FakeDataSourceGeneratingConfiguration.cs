namespace Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

public record FakeDataSourceGeneratingConfiguration
{
    public string[] AllowedCurrenciesIso4217 { get; init; } = [];
    public string FakeDataSourceId { get; init; } = string.Empty;
}
