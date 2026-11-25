using System.Security.Claims;
using Api.Extension;
using Application.Dtos.Estantes.Requests;
using Application.Dtos.Estantes.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Estantes;

public static class EstanteEndpoint
{
    public static void MapEstanteEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/estantes")
            .WithTags("Estantes")
            .RequireAuthorization();

        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IEstanteService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.ObterEstantesUsuarioAsync(idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterMinhasEstantes")
        .WithSummary("Obtém as estantes Favoritos e Pretende Jogar do jogador autenticado")
        .Produces<EstantesUsuarioResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/pretende-jogar/promocoes", async (
            ClaimsPrincipal user,
            IEstanteService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.ObterPromocoesPretendeJogarAsync(idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterPromocoesPretendeJogar")
        .WithSummary("Verifica promoções ativas em tempo real para os jogos da estante Pretende Jogar")
        .WithDescription("Consulta o cache temporário de ofertas no Redis (30 min) ou atualiza ao vivo na Steam Storefront API sem depender de preços antigos no banco SQL.")
        .Produces<PromocoesPretendeJogarResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapPost("/{tipoEstante}/jogos/{steamAppId:int}", async (
            string tipoEstante,
            int steamAppId,
            ClaimsPrincipal user,
            IEstanteService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var request = new AlterarJogoEstanteRequestDto
            {
                TipoEstante = tipoEstante,
                SteamAppId = steamAppId
            };

            var resposta = await service.AdicionarJogoAsync(idUsuario, request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("AdicionarJogoEstante")
        .WithSummary("Adiciona um jogo em 'favoritos' ou 'pretende-jogar'")
        .WithDescription("Sincroniza metadados fixos no SQL Server, guarda a oferta volátil no Redis e atualiza JogoLogger, GeneroLogger e UsuarioLogger.")
        .Produces<EstantesUsuarioResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{tipoEstante}/jogos/{steamAppId:int}", async (
            string tipoEstante,
            int steamAppId,
            ClaimsPrincipal user,
            IEstanteService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var request = new AlterarJogoEstanteRequestDto
            {
                TipoEstante = tipoEstante,
                SteamAppId = steamAppId
            };

            var resposta = await service.RemoverJogoAsync(idUsuario, request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("RemoverJogoEstante")
        .WithSummary("Remove um jogo de 'favoritos' ou 'pretende-jogar'")
        .Produces<EstantesUsuarioResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/favoritos/configuracao", async (
            AtualizarFavoritosConfigRequestDto request,
            ClaimsPrincipal user,
            IEstanteService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AtualizarConfiguracaoFavoritosAsync(idUsuario, request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("AtualizarConfiguracaoFavoritos")
        .WithSummary("Atualiza atributos próprios da estante Favoritos")
        .Produces<EstanteFavoritosResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/pretende-jogar/configuracao", async (
            AtualizarPretendeJogarConfigRequestDto request,
            ClaimsPrincipal user,
            IEstanteService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AtualizarConfiguracaoPretendeJogarAsync(idUsuario, request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("AtualizarConfiguracaoPretendeJogar")
        .WithSummary("Atualiza atributos próprios da estante Pretende Jogar")
        .Produces<EstantePretendeJogarResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
