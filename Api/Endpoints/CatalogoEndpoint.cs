using System.Security.Claims;
using Api.Extension;
using Application.Dtos.Catalogo.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Catalogo;

public static class CatalogoEndpoint
{
    public static void MapCatalogoEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/jogos")
            .WithTags("Catálogo");

        group.MapGet("/busca", async (
            string termo,
            string? tipo,
            string? algoritmo,
            ClaimsPrincipal user,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.BuscarAsync(tipo, termo, algoritmo, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("BuscarJogosCatalogo")
        .WithSummary("Busca polimórfica de jogos no catálogo")
        .WithDescription("Realiza busca polimórfica por nome, ano ou autor (desenvolvedora/publicadora) e ordena pelo algoritmo escolhido (popularidade, tendencia ou custo-beneficio).")
        .Produces<BuscaCatalogoResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/destaques", async (
            string? algoritmo,
            ClaimsPrincipal user,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ObterDestaquesAsync(algoritmo, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterDestaquesCatalogo")
        .WithSummary("Obtém as vitrines de destaque da Steam")
        .WithDescription("Retorna Mais Vendidos, Lançamentos, Ofertas Especiais e Em Breve ordenados pelo algoritmo selecionado.")
        .Produces<DestaquesCatalogoResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/tendencias/generos", async (
            ClaimsPrincipal user,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ObterTendenciasGenerosAsync(idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterTendenciasGeneros")
        .WithSummary("Obtém o termômetro de tendências por gênero (GeneroLogger)")
        .WithDescription("Retorna a pontuação acumulada de engajamento por gênero na comunidade e, se autenticado, a afinidade individual do jogador.")
        .Produces<TendenciasGenerosResponseDto>(StatusCodes.Status200OK);

        group.MapGet("/comunidade/usuarios-populares", async (
            int? limite,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var resposta = await service.ObterUsuariosPopularesAsync(limite ?? 10, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterUsuariosPopularesComunidade")
        .WithSummary("Lista os jogadores mais populares e seus últimos relatos (UsuarioLogger)")
        .WithDescription("Ranqueia os membros com maior engajamento na comunidade Checkpoint acompanhados de seus relatos recentes.")
        .Produces<IReadOnlyList<UsuarioPopularResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{steamAppId:int}", async (
            int steamAppId,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var resposta = await service.ObterDetalhesPorSteamAppIdAsync(steamAppId, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterDetalhesJogoPorSteamAppId")
        .WithSummary("Obtém detalhes completos de um jogo e suas métricas no JogoLogger")
        .WithDescription("Sincroniza sob demanda os metadados da Steam Storefront API em pt-BR e retorna junto com os indicadores da comunidade Checkpoint.")
        .Produces<JogoDetalhesResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}
