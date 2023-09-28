using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using APIContagem.Kafka;
using APIContagem.Tracing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Documentacao do OpenTelemetry:
// https://opentelemetry.io/docs/instrumentation/net/getting-started/

// Integracao do OpenTelemetry com Jaeger:
// https://opentelemetry.io/docs/instrumentation/net/exporters/

// Documentacaoo do Jaeger:
// https://www.jaegertracing.io/docs/1.49/

builder.Services.AddOpenTelemetry().WithTracing(traceProvider =>
{
    traceProvider
        .AddSource(OpenTelemetryExtensions.ServiceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: OpenTelemetryExtensions.ServiceName,
                    serviceVersion: OpenTelemetryExtensions.ServiceVersion))
        .AddAspNetCoreInstrumentation()
        .AddJaegerExporter(exporter =>
        {
            exporter.AgentHost = builder.Configuration["Jaeger:AgentHost"];
            exporter.AgentPort = Convert.ToInt32(builder.Configuration["Jaeger:AgentPort"]);
        })
        .AddConsoleExporter();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (int.TryParse(app.Configuration["ApacheKafka:WaitingTimeInitialization"],
        out int waitingTimeInitialization) && waitingTimeInitialization > 0)
{
    // Tempo a ser aguardado para garantir que a instância do Kafka esteja disponível
    // em testes com containers
    app.Logger.LogInformation(
        $"Aguardando {waitingTimeInitialization} milissegundos para iniciar a aplicacao...");
    await Task.Delay(waitingTimeInitialization);
}
// Força a comunicação com o Apache Kafka para criar o tópico
app.Logger.LogInformation(
    $"Verificando a necessida de criacao do topico {app.Configuration["ApacheKafka:Topic"]}...");
KafkaExtensions.CheckNumPartitions(builder.Configuration);

app.Run();