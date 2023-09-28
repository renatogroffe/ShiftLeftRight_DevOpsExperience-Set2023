using Microsoft.Data.SqlClient;
using Dapper.Contrib.Extensions;
using WorkerContagem.Models;

namespace WorkerContagem.Data;

public class ContagemRepository
{
    private readonly IConfiguration _configuration;

    public ContagemRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Save(ResultadoContador resultado, int partition)
    {
        using var conexao = new SqlConnection(
            _configuration.GetConnectionString("BaseContagem"));
        conexao.Insert<HistoricoContagem>(new()
        {
            DataProcessamento = DateTime.UtcNow.AddHours(-3), // Horário padrão do Brasil
            ValorAtual = resultado.ValorAtual,
            Producer = resultado.Producer,
            Consumer = Environment.MachineName,
            TopicName = _configuration["ApacheKafka:Topic"],
            PartitionNumber = partition,
            Mensagem = resultado.Mensagem,
            Kernel = resultado.Kernel,
            Framework = resultado.Framework
        });
    }
}