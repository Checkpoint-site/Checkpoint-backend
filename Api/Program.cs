using DotNetEnv;
using Api.Endpoints.Cadastro;
using Api.Endpoints.Login;
using Api.Extension;
using Api.Middleware;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddConexaoBanco(builder.Configuration)
                .AddInjecaoDependencia();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var api = app.MapGroup("/api/v1")
    .WithTags("API");

api.MapLoginEndpoint();
api.MapCadastroEndpoint();

app.Run();
