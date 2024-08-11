using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

namespace Wachhund.Infrastructure.FakeSource.DataSourcing;

public class FakeDataSource : IFakeDataSource
{
    private readonly IFakeTradeDealGenerator _generator;

    public FakeDataSource(IFakeTradeDealGenerator generator)
    {
        _generator = generator;
    }

    public IAsyncEnumerable<TradeDeal> FetchDataAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }




}
