using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevForge.ControlPlane.Application;
using DevForge.ControlPlane.Infrastructure;
using DevForge.ControlPlane.MonitoringWorker.Consumers;
using DevForge.ControlPlane.MonitoringWorker;
using DevForge.ControlPlane.Application.Common.Metrics;
using MassTransit;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.UseUrls("http://*:8085");

// 1. Add Clean Architecture Layers dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure OpenTelemetry Metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter(DevForgeMetrics.MeterName)
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

builder.Services.AddHostedService<ContainerMetricsWorker>();

// 2. Configure MassTransit with RabbitMQ and Telemetry Consumer
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PipelineTelemetryConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("devforge-monitoring-jobs", e =>
        {
            e.ConfigureConsumer<PipelineTelemetryConsumer>(context);
        });
    });
});

var app = builder.Build();

// Seed system monitoring project/job
await DevForge.ControlPlane.Infrastructure.Persistence.DbInitializer.SeedSystemProjectAsync(app.Services);

app.MapPrometheusScrapingEndpoint();

app.Run();
