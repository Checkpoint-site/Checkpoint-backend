using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Persistence.Services;

namespace Test.Persistence.Services;

public class OpenAiModeracaoFotoServiceTests
{
    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public string? UltimoPayloadEnviado { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                UltimoPayloadEnviado = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return responder(request);
        }
    }

    [Fact]
    public async Task FotoEhSeguraAsync_Base64SemPrefixo_FormataDataUriEAvaliaFlaggedCorretamente()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"results":[{"flagged":true}]}""", Encoding.UTF8, "application/json")
        });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OpenAI:ApiKey"] = "sk-test-key" })
            .Build();

        var service = new OpenAiModeracaoFotoService(new HttpClient(handler), config);

        bool segura = await service.FotoEhSeguraAsync("iVBORw0KGgoAAAANSUhEUgAAAAUA");

        Assert.False(segura);
        Assert.NotNull(handler.UltimoPayloadEnviado);
        Assert.Contains("data:image/jpeg;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA", handler.UltimoPayloadEnviado);
    }

    [Fact]
    public async Task FotoEhSeguraAsync_SemFotoOuSemChave_RetornaTrue()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("Não deveria chamar HTTP"));
        var config = new ConfigurationBuilder().Build();
        var service = new OpenAiModeracaoFotoService(new HttpClient(handler), config);

        Assert.True(await service.FotoEhSeguraAsync(null));
        Assert.True(await service.FotoEhSeguraAsync("https://cdn.checkpoint.com/avatar.png"));
    }
}
