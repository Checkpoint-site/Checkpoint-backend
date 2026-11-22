using Application.Dtos.Login.Requests;
using Application.Dtos.Login.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/login")
            .WithTags("Autenticação");

        group.MapPost("", async (
            LoginRequestDto request,
            ILoginService Service) =>
        {
            var resposta = await Service.AutenticarAsync(request);
            return TypedResults.Ok(resposta);
        })
        .WithName("Login")
        .WithSummary("Realiza a autenticação do usuário")
        .WithDescription("Autentica o jogador através de e-mail e senha, retornando o token JWT e dados do perfil.")
        .Produces<LoginResponseDto>(200)
        .ProducesProblem(401)
        .ProducesProblem(400)
        .ProducesProblem(404);

    }
}
