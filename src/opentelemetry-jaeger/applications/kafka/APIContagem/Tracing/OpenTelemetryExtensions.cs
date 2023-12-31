﻿using System.Diagnostics;

namespace APIContagem.Tracing;

public static class OpenTelemetryExtensions
{
    public static string ServiceName { get; }
    public static string ServiceVersion { get; }

    static OpenTelemetryExtensions()
    {
        ServiceName = typeof(OpenTelemetryExtensions).Assembly.GetName().Name! + "Kafka";
        ServiceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version!.ToString();
    }

    public static ActivitySource CreateActivitySource() =>
        new ActivitySource(ServiceName, ServiceVersion);
}