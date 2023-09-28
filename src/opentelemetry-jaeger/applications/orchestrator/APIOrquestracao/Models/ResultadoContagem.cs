namespace APIOrquestracao.Models;

public class ResultadoOrquestracao
{
    public string? Horario { get; set; }
    public ResultadoContagem? ContagemRedis { get; set; }
    public ResultadoContagem? ContagemKafka { get; set; }
}

public class ResultadoContagem
{
    public int ValorAtual { get; set; }
    public string? Producer { get; set; }
    public string? Kernel { get; set; }
    public string? Framework { get; set; }
    public string? Mensagem { get; set; }
}