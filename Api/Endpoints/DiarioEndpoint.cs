using System.Security.Claims;
using Api.Extension;
using Application.Dtos.Diario.Requests;
using Application.Dtos.Diario.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Diario;

public static class DiarioEndpoint
{
    public static void MapDiarioEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/diario")
            .WithTags("Diário & Biblioteca")
            .RequireAuthorization();

        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IDiarioService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.ObterMinhaBibliotecaAsync(idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterMeuDiarioBiblioteca")
        .WithSummary("Obtém todos os registros de jogos, notas de fogueiras, estantes e reviews do jogador autenticado")
        .Produces<BibliotecaUsuarioResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/jogos/{steamAppId:int}/avaliacao", async (
            int steamAppId,
            SalvarAvaliacaoJogoRequestDto request,
            ClaimsPrincipal user,
            IDiarioService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.SalvarAvaliacaoJogoAsync(idUsuario, steamAppId, request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("SalvarAvaliacaoJogoDiario")
        .WithSummary("Registra ou atualiza status, nota de fogueiras (0.5 a 5.0), favorito, tags e review opcional")
        .WithDescription("Sincroniza o jogo com a Steam se necessário, recalcula a média e distribuição de fogueiras do jogo e publica/atualiza a review caso haja texto.")
        .Produces<AvaliacaoSalvaResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/jogos/{steamAppId:int}/jogado", async (
            int steamAppId,
            ClaimsPrincipal user,
            IDiarioService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AlternarJogadoAsync(idUsuario, steamAppId, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("AlternarJogoJogadoDiario")
        .WithSummary("Marca ou desmarca rapidamente um jogo como jogado no diário do usuário")
        .Produces<RegistroJogoUsuarioResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
