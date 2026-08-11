using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevForge.ControlPlane.Application;
using DevForge.ControlPlane.Infrastructure;
using DevForge.ControlPlane.RepairWorker.Consumers;
using DevForge.ControlPlane.Application.Common.Metrics;
using MassTransit;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.UseUrls("http://*:8094");

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

// 2. Configure MassTransit with RabbitMQ and Consumers
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<QaFailedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("devforge-repair-jobs", e =>
        {
            e.ConfigureConsumer<QaFailedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();

app.Run();
