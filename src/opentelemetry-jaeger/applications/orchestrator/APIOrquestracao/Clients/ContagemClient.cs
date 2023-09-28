using System.Net.Http.Headers;
using APIOrquestracao.Models;

namespace APIOrquestracao.Clients;

public class ContagemClient
{
    private readonly HttpClient _client;

    public ContagemClient(HttpClient client)
    {
        _client = client;
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
    }

    public async Task<ResultadoContagem?> ObterContagemAsync(string urlApi) =>
        await _client.GetFromJsonAsync<ResultadoContagem>(urlApi);
}