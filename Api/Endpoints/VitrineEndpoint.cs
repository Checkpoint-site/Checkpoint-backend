using System.Security.Claims;
using Api.Extension;
using Application.Dtos.Vitrine.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Vitrine;

public static class VitrineEndpoint
{
    public static void MapVitrineEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/vitrine")
            .WithTags("Vitrine");

        group.MapGet("/home", async (
            ClaimsPrincipal user,
            IVitrineService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ObterVitrineHomeAsync(idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterVitrineHome")
        .WithSummary("Retorna a vitrine principal completa (destaques, gêneros, Top 10 e reviews da Fogueira)")
        .Produces<VitrineHomeResponseDto>(StatusCodes.Status200OK);

        group.MapGet("/fallback", async (
            int? limite,
            string? algoritmo,
            ClaimsPrincipal user,
            IVitrineService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ObterVitrineFallbackAsync(
                limite ?? 24,
                algoritmo,
                idUsuario,
                cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterVitrineFallback")
        .WithSummary("Retorna vitrine dinâmica de jogos aleatórios com relevância quando o banco está vazio")
        .WithDescription("Monta destaques, Top 10 e carrosséis por gênero combinando sinais reais de relevância na Steam (mais vendidos, ofertas, lançamentos, Metacritic e recomendações) com amostragem aleatória controlada, sem poluir o banco SQL Server.")
        .Produces<VitrineHomeResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
