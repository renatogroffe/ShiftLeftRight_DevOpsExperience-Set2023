using Microsoft.AspNetCore.Mvc;
using APIOrquestracao.Models;
using APIOrquestracao.Clients;
using System.Diagnostics;
using APIOrquestracao.Tracing;

namespace APIOrquestracao.Controllers;

[ApiController]
[Route("[controller]")]
public class OrquestracaoController : ControllerBase
{
    private readonly ILogger<OrquestracaoController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ContagemClient _contagemClient;
    private readonly ActivitySource _activitySource;

    public OrquestracaoController(ILogger<OrquestracaoController> logger,
        IConfiguration configuration, ContagemClient contagemClient)
    {
        _activitySource = OpenTelemetryExtensions.CreateActivitySource();
        using var activity =
            _activitySource.StartActivity($"Construtor ({nameof(OrquestracaoController)})");
        activity!.SetTag("horario", $"{DateTime.Now:HH:mm:ss dd/MM/yyyy}");

        _logger = logger;
        _configuration = configuration;
        _contagemClient = contagemClient;
    }

    [HttpGet]
    public async Task<ResultadoOrquestracao> Get()
    {
        using var activity =
            _activitySource.StartActivity($"{nameof(Get)} ({nameof(OrquestracaoController)})");

        var resultado = new ResultadoOrquestracao();
        resultado.Horario = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var urlApiRedis = _configuration["ApiContagemRedis"]!;
        resultado.ContagemRedis =
            await _contagemClient.ObterContagemAsync(urlApiRedis);
        _logger.LogInformation($"Valor contagem Redis: {resultado.ContagemRedis!.ValorAtual}");
        
        var urlApiKafka = _configuration["ApiContagemKafka"]!;
        resultado.ContagemKafka =
            await _contagemClient.ObterContagemAsync(urlApiKafka);
        _logger.LogInformation($"Valor contagem Kafka: {resultado.ContagemKafka!.ValorAtual}");

        activity!.SetTag("urlApiRedis", urlApiRedis);
        activity!.SetTag("urlApiKafka", urlApiKafka);
        activity!.SetTag("resultadoOrquestracao", resultado);

        return resultado;
    }
}
