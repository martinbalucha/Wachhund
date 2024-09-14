using Serilog;
using Wachhund.Application.Server.Services;
using Wachhund.Contracts.TradeDetection;
using Wachhund.Contracts.TradeDetection.Persistence;
using Wachhund.Domain;
using Wachhund.Domain.Detection.Caching;
using Wachhund.Infrastructure.FakeSource;
using Wachhund.Infrastructure.FakeSource.DataSourcing;
using Wachhund.Infrastructure.FakeSource.TradeDealGenerating;

namespace Wachhund;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("loggerConfig.json");

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSerilog(ctx => ctx.ReadFrom.Configuration(builder.Configuration));

        // Domain
        builder.Services.Configure<SuspiciousDealDetectorConfiguration>(builder.Configuration
            .GetRequiredSection(nameof(SuspiciousDealDetectorConfiguration)));

        builder.Services.AddSingleton<ISuspiciousDealDetector, SuspicousDealDetector>();

        // Fake source
        builder.Services.Configure<FakeDataSourceConfiguration>(builder.Configuration
            .GetRequiredSection(nameof(FakeDataSourceConfiguration)));

        builder.Services.Configure<FakeDataSourceGeneratingConfiguration>(builder.Configuration
            .GetRequiredSection(nameof(FakeDataSourceGeneratingConfiguration)));

        builder.Services.AddSingleton<IFakeDataSource, FakeDataSource>();
        builder.Services.AddSingleton<IFakeTradeDealGenerator, BogusTradeDealGenerator>();

        // Persistence
        builder.Services.AddSingleton<ITradeDealCache, InMemoryTradeDealCache>();

        // Monitoring
        builder.Services.AddSingleton<IMonitor, FakeMonitor>();

        builder.Services.AddHostedService<MonitoringService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
