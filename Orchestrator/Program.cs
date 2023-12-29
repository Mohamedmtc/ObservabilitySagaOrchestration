using Core.RabbitMq.BusConfiguration;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orchestrator.Presistance;

using Orchestrator.StateMachine.Order;
using Serilog;
using System;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



#region MassTransit
builder.Services.AddDbContext<OrchSagaDbContext>((provider, dbContextBuilder) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    dbContextBuilder.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(OrchSagaDbContext).Assembly.FullName);
        sqlOptions.MigrationsHistoryTable($"__{nameof(OrchSagaDbContext)}");
    });
});

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderStateData>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // or use Optimistic, which requires RowVersion
                    r.ExistingDbContext<OrchSagaDbContext>();

                });

    cfg.AddBus(provider => RabbitMqBus.ConfigureBusWebApi(provider, builder.Configuration));
});
//builder.Services.AddMassTransitHostedService(true);
#endregion



#region OpenTelemetry
string servicename = builder.Configuration.GetValue<string>("Otlp:ServiceName") ?? "";
string otlpENdPoint = builder.Configuration.GetValue<string>("Otlp:Endpoint") ?? "";
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(hostingContext.Configuration)
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = $"{otlpENdPoint}/v1/logs";
                options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.GrpcProtobuf;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = servicename
                };
            }));




Action<ResourceBuilder> appResourceBuilder =
    resource => resource
        .AddTelemetrySdk()
        .AddService(builder.Configuration.GetValue<string>("Otlp:ServiceName") ?? "");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(appResourceBuilder)
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMassTransitInstrumentation()
        .AddSource(DiagnosticHeaders.DefaultListenerName)
        .AddSource("APITracing")
        //.AddConsoleExporter()
        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpENdPoint))
    )
    .WithMetrics(builder => builder
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpENdPoint)));

#endregion




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
