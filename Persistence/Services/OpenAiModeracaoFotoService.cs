using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Persistence.Services;

public class OpenAiModeracaoFotoService(HttpClient httpClient, IConfiguration configuration) : IModeracaoFotoService
{
    private const string MODERATION_URL = "https://api.openai.com/v1/moderations";

    public async Task<bool> FotoEhSeguraAsync(string? urlOuBase64)
    {
        if (string.IsNullOrWhiteSpace(urlOuBase64))
        {
            return true;
        }

        string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return true;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, MODERATION_URL);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            string imagemFormatada = NormalizarEntradaImagem(urlOuBase64);

            var payload = new
            {
                model = "omni-moderation-latest",
                input = new object[]
                {
                    new
                    {
                        type = "image_url",
                        image_url = new { url = imagemFormatada }
                    }
                }
            };

            string jsonBody = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);

            if (doc.RootElement.TryGetProperty("results", out var results) &&
                results.GetArrayLength() > 0 &&
                results[0].TryGetProperty("flagged", out var flagged))
            {
                return !flagged.GetBoolean();
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    private static string NormalizarEntradaImagem(string entrada)
    {
        string valor = entrada.Trim();

        if (valor.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            valor.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            valor.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            return valor;
        }

        return $"data:image/jpeg;base64,{valor}";
    }
}
