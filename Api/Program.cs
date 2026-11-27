using Api.Endpoints.Cadastro;
using Api.Endpoints.Catalogo;
using Api.Endpoints.Diario;
using Api.Endpoints.Estantes;
using Api.Endpoints.Login;
using Api.Endpoints.Moderacao;
using Api.Endpoints.Posts;
using Api.Endpoints.Vitrine;
using Api.Extension;
using Api.Middleware;
using DotNetEnv;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer()
                .AddConfiguracaoSwagger()
                .AddConfiguracaoCors(builder.Configuration)
                .AddConexaoBanco(builder.Configuration)
                .AddConexaoRedis(builder.Configuration)
                .AddInjecaoDependencia()
                .AddAutenticacaoSeguranca(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(ConfiguracaoCors.NomePoliticaPadrao);

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api/v1")
    .WithTags("API");

api.MapLoginEndpoint();
api.MapCadastroEndpoint();
api.MapModeracaoEndpoint();
api.MapCatalogoEndpoint();
api.MapEstanteEndpoint();
api.MapPostEndpoint();
api.MapDiarioEndpoint();
api.MapVitrineEndpoint();

app.Run();
