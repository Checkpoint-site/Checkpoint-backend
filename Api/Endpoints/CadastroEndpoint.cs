using Application.Dtos.Cadastro.Requests;
using Application.Dtos.Cadastro.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Cadastro;

public static class CadastroEndpoint
{
    public static void MapCadastroEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cadastro")
            .WithTags("Autenticação");

        group.MapPost("", async (
            CadastroRequestDto request,
            ICadastroService service) =>
        {
            var resposta = await service.CadastrarAsync(request);
            return TypedResults.Ok(resposta);
        })
        .WithName("Cadastro")
        .WithSummary("Cadastra um novo usuário no sistema")
        .WithDescription("Registra um novo jogador com nome, e-mail e senha.")
        .Produces<CadastroResponseDto>(201)
        .ProducesProblem(400)
        .ProducesProblem(409);
    }
}
