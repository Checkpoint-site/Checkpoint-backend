using Api.Endpoints.Cadastro;
using Api.Endpoints.Catalogo;
using Api.Endpoints.Estantes;
using Api.Endpoints.Login;
using Api.Endpoints.Moderacao;
using Api.Extension;
using Api.Middleware;
using DotNetEnv;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer()
                .AddConfiguracaoSwagger()
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api/v1")
    .WithTags("API");

api.MapLoginEndpoint();
api.MapCadastroEndpoint();
api.MapModeracaoEndpoint();
api.MapCatalogoEndpoint();
api.MapEstanteEndpoint();

app.Run();
