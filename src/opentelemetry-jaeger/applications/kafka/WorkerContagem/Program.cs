using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WorkerContagem;
using WorkerContagem.Data;
using WorkerContagem.Kafka;
using WorkerContagem.Tracing;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<ContagemRepository>();
        services.AddOpenTelemetry().WithTracing(traceProvider =>
        {
            traceProvider
                .AddSource(OpenTelemetryExtensions.ServiceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: OpenTelemetryExtensions.ServiceName,
                            serviceVersion: OpenTelemetryExtensions.ServiceVersion))
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation(
                    options => options.SetDbStatementForText = true)
                .AddJaegerExporter(exporter =>
                {
                    exporter.AgentHost = hostContext.Configuration["Jaeger:AgentHost"];
                    exporter.AgentPort = Convert.ToInt32(hostContext.Configuration["Jaeger:AgentPort"]);
                })
                .AddConsoleExporter();
        });

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();