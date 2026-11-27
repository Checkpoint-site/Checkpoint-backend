using Application.Dtos.Catalogo.Requests;
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
            string jogoBuscado,
            string? tipo,
            string? algoritmo,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var request = new BuscarCatalogoRequestDto
            {
                JogoBuscado = jogoBuscado,
                Tipo = tipo,
                Algoritmo = algoritmo
            };

            var resposta = await service.BuscarAsync(request, cancellationToken);
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
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterDestaquesCatalogoRequestDto
            {
                Algoritmo = algoritmo
            };

            var resposta = await service.ObterDestaquesAsync(request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterDestaquesCatalogo")
        .WithSummary("Obtém as vitrines de destaque da Steam")
        .WithDescription("Retorna Mais Vendidos, Lançamentos, Ofertas Especiais e Em Breve ordenados pelo algoritmo selecionado.")
        .Produces<DestaquesCatalogoResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/aleatorios", async (
            int? limite,
            string? algoritmo,
            bool? forcarFallbackSteam,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterJogosAleatoriosRequestDto
            {
                Limite = limite ?? 12,
                Algoritmo = algoritmo,
                ForcarFallbackSteam = forcarFallbackSteam ?? false
            };

            var resposta = await service.ObterJogosAleatoriosAsync(request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterJogosAleatoriosRelevantes")
        .WithSummary("Retorna jogos aleatórios ponderados por relevância (com fallback automático na Steam caso o banco esteja vazio)")
        .WithDescription("Seleciona jogos combinando curadoria de relevância (popularidade, tendência ou custo-benefício) e amostragem aleatória controlada. Quando não há jogos suficientes no banco local, consulta automaticamente o pool relevante da Steam sem poluir o banco.")
        .Produces<DescobertaCatalogoResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/tendencias/generos", async (
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var resposta = await service.ObterTendenciasGenerosAsync(cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterTendenciasGeneros")
        .WithSummary("Obtém o termômetro global de tendências por gênero (GeneroLogger)")
        .WithDescription("Retorna a pontuação e os contadores acumulados dos 12 gêneros da comunidade.")
        .Produces<TendenciasGenerosResponseDto>(StatusCodes.Status200OK);

        group.MapGet("/comunidade/usuarios-populares", async (
            int? limite,
            ICatalogoService service,
            CancellationToken cancellationToken) =>
        {
            var request = new ObterUsuariosPopularesRequestDto
            {
                Limite = limite ?? 10
            };

            var resposta = await service.ObterUsuariosPopularesAsync(request, cancellationToken);
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
            var request = new ObterDetalhesJogoRequestDto
            {
                SteamAppId = steamAppId
            };

            var resposta = await service.ObterDetalhesPorSteamAppIdAsync(request, cancellationToken);
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
