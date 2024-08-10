using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wachhund.Contracts.TradeDetection;

namespace Wachhund.Infrastructure.FakeSource;

public class FakeMonitor : IMonitor
{
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
