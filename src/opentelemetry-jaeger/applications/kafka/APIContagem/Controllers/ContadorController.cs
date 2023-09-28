using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using APIContagem.Models;
using APIContagem.Kafka;
using APIContagem.Tracing;

namespace APIContagem.Controllers;

[ApiController]
[Route("[controller]")]
public class ContadorController : ControllerBase
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private static readonly Contador _CONTADOR = new();
    private readonly ILogger<ContadorController> _logger;
    private readonly IConfiguration _configuration;

    public ContadorController(ILogger<ContadorController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public ResultadoContador Get()
    {
        // Solucao que serviu de base para a implementacao deste exemplo:
        // https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/examples/MicroserviceExample

        int valorAtualContador;
        int partition;

        lock (_CONTADOR)
        {
            _CONTADOR.Incrementar();
            valorAtualContador = _CONTADOR.ValorAtual;
            partition = _CONTADOR.Partition;
        }

        using var activity = OpenTelemetryExtensions.CreateActivitySource()
            .StartActivity("Identificando");
        activity?.SetTag("valorAtual", valorAtualContador);

        var resultado = new ResultadoContador()
        {
            ValorAtual = valorAtualContador,
            Producer = _CONTADOR.Local,
            Kernel = _CONTADOR.Kernel,
            Framework = _CONTADOR.Framework,
            Mensagem = _configuration["MensagemVariavel"]
        };

        string topicName = _configuration["ApacheKafka:Topic"]!;
        string jsonContagem = JsonSerializer.Serialize(resultado);

        // Semantic convention - OpenTelemetry messaging specification:
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
        var activityName = $"{topicName} send";
        
        using (var producer = KafkaExtensions.CreateProducer(_configuration))
        {
            var idMensagemContagem = Guid.NewGuid().ToString();

            using var sendActivity = OpenTelemetryExtensions.CreateActivitySource()
                .StartActivity(activityName, ActivityKind.Producer);

            ActivityContext contextToInject = default;
            if (sendActivity != null)
            {
                contextToInject = sendActivity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            var headers = new Headers();
            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), headers,
                InjectTraceContextIntoHeaders);

            sendActivity?.SetTag("messaging.system", "kafka");
            sendActivity?.SetTag("messaging.destination_kind", "topic");
            sendActivity?.SetTag("messaging.destination", topicName);
            sendActivity?.SetTag("messaging.operation", "process");
            sendActivity?.SetTag("messaging.kafka.client_id",
                $"{nameof(APIContagem)}-{Environment.MachineName}");
            sendActivity?.SetTag("message", jsonContagem);
            sendActivity?.SetTag("idMensagemContagem", idMensagemContagem);
            sendActivity?.SetTag("valorAtualContador", valorAtualContador);

            var result = producer.ProduceAsync(
                new TopicPartition(topicName, new Partition(partition)),
                new Message<Null, string>
                { Value = jsonContagem, Headers = headers }).Result;

            _logger.LogInformation(
                $"Apache Kafka - Envio para o topico {topicName} concluido | Particao: {partition} | " +
                $"{jsonContagem} | Id Mensagem: {idMensagemContagem} | Status: {result.Status}");
        }

        return resultado;
    }

    private void InjectTraceContextIntoHeaders(Headers headers, string key, string value)
    {
        try
        {
            headers.Add(key, Encoding.UTF8.GetBytes(value));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Falha de injecao com o trace context.");
        }
    }
}