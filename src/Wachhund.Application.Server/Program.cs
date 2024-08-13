using Serilog;
using Wachhund.Application.Server.Services;
using Wachhund.Contracts.TradeDetection.Persistence;
using Wachhund.Domain;
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

        // Fake source
        builder.Services.Configure<FakeDataSourceConfiguration>(builder.Configuration
            .GetRequiredSection(nameof(FakeDataSourceConfiguration)));

        builder.Services.Configure<FakeDataSourceGeneratingConfiguration>(builder.Configuration
            .GetRequiredSection(nameof(FakeDataSourceGeneratingConfiguration)));

        builder.Services.AddSingleton<IFakeDataSource, FakeDataSource>();
        builder.Services.AddSingleton<IFakeTradeDealGenerator, BogusTradeDealGenerator>();
        builder.Services.AddSingleton<FakeMonitor>();

        // Persistence
        builder.Services.AddSingleton<ITradeDealCache, InMemoryTradeDealCache>();

        // Monitoring process
        builder.Services.AddHostedService<MonitoringService>(services =>
        {
            var monitors = services.GetRequiredService<FakeMonitor>();

            var logger = services.GetRequiredService<ILogger<MonitoringService>>();

            return new MonitoringService(new[] { monitors }, logger);
        });

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
