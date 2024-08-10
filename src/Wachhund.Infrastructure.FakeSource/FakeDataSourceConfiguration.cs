using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wachhund.Infrastructure.FakeSource;

public record FakeDataSourceConfiguration
{
    public string[] AllowedCurrenciesIso4217 { get; init; } = [];
}
