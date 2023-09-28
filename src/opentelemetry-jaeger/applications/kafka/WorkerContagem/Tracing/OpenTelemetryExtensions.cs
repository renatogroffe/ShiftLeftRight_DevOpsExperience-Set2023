using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WorkerContagem.Tracing;

public static class OpenTelemetryExtensions
{
    public static string Local { get; }
    public static string Kernel { get; }
    public static string Framework { get; }
    public static string ServiceName { get; }
    public static string ServiceVersion { get; }

    static OpenTelemetryExtensions()
    {
        Local = Environment.MachineName;
        Kernel = Environment.OSVersion.VersionString;
        Framework = RuntimeInformation.FrameworkDescription;
        ServiceName = typeof(OpenTelemetryExtensions).Assembly.GetName().Name! + "Kafka";
        ServiceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version!.ToString();
    }

    public static ActivitySource CreateActivitySource() =>
        new(ServiceName, ServiceVersion);
}