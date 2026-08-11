using System.Diagnostics.Metrics;

namespace DevForge.ControlPlane.Application.Common.Metrics;

public static class DevForgeMetrics
{
    public const string MeterName = "DevForge.ControlPlane";
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    // Process/pipeline duration metrics
    public static readonly Histogram<double> GenerationDuration = Meter.CreateHistogram<double>(
        "devforge_generation_duration_seconds",
        "Seconds",
        "Duration of the application generation process in seconds");

    public static readonly Histogram<double> QaDuration = Meter.CreateHistogram<double>(
        "devforge_qa_duration_seconds",
        "Seconds",
        "Duration of the QA validation process in seconds");

    public static readonly Histogram<double> DeploymentDuration = Meter.CreateHistogram<double>(
        "devforge_deployment_duration_seconds",
        "Seconds",
        "Duration of the application deployment process in seconds");

    public static readonly Histogram<double> RepairDuration = Meter.CreateHistogram<double>(
        "devforge_repair_duration_seconds",
        "Seconds",
        "Duration of the self-healing repair process in seconds");

    public static readonly Histogram<double> LeadTimeToLive = Meter.CreateHistogram<double>(
        "devforge_lead_time_to_live_seconds",
        "Seconds",
        "Total lead time from project creation to application live");

    // Container metrics (Sprint 9D)
    public static readonly Histogram<double> ContainerCpuUsage = Meter.CreateHistogram<double>(
        "devforge_container_cpu_usage",
        "Percent",
        "Container CPU usage percentage");

    public static readonly Histogram<double> ContainerMemoryUsage = Meter.CreateHistogram<double>(
        "devforge_container_memory_usage_bytes",
        "Bytes",
        "Container memory usage in bytes");

    public static readonly Counter<long> ContainerRestartCount = Meter.CreateCounter<long>(
        "devforge_container_restart_count",
        "Count",
        "Container restart count");

    public static readonly Histogram<double> ContainerUptime = Meter.CreateHistogram<double>(
        "devforge_container_uptime_seconds",
        "Seconds",
        "Container uptime in seconds");
}
