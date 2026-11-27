using System.Security.Claims;
using Api.Extension;
using Application.Dtos.Posts.Requests;
using Application.Dtos.Posts.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Posts;

public static class PostEndpoint
{
    public static void MapPostEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/posts")
            .WithTags("Posts & Reviews");

        group.MapGet("", async (
            int? limite,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ListarRecentesAsync(limite ?? 20, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ListarPostsRecentes")
        .WithSummary("Lista as reviews mais recentes publicadas na Fogueira")
        .Produces<IReadOnlyList<PostResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/jogo/{steamAppId:int}", async (
            int steamAppId,
            string? ordenacao,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ListarPorJogoSteamAppIdAsync(steamAppId, ordenacao, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ListarPostsPorJogo")
        .WithSummary("Lista as reviews de um jogo ordenadas por recentes, curtidas ou nota")
        .Produces<IReadOnlyList<PostResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{idPost:int}", async (
            int idPost,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ObterDetalhesComComentariosAsync(idPost, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ObterPostComComentarios")
        .WithSummary("Obtém uma review completa com seus comentários e outras conversas da Fogueira")
        .Produces<PostComComentariosResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/jogo/{steamAppId:int}", async (
            int steamAppId,
            SalvarPostRequestDto request,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.SalvarPorJogoSteamAppIdAsync(idUsuario, steamAppId, request, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization()
        .WithName("SalvarPostDoJogo")
        .WithSummary("Publica ou atualiza a review do jogador autenticado para um jogo")
        .Produces<PostResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{idPost:int}", async (
            int idPost,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var role = user.ObterRoleOpcional();
            await service.ExcluirAsync(idPost, idUsuario, role, cancellationToken);
            return TypedResults.NoContent();
        })
        .RequireAuthorization()
        .WithName("ExcluirPost")
        .WithSummary("Exclui uma review (autor ou moderador/admin) e recalcula a média de fogueiras")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{idPost:int}/curtir", async (
            int idPost,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AlternarCurtidaPostAsync(idPost, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization()
        .WithName("AlternarCurtidaPost")
        .WithSummary("Curte ou descurte uma review da comunidade")
        .Produces<CurtidaResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{idPost:int}/comentarios", async (
            int idPost,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioOpcional();
            var resposta = await service.ListarComentariosAsync(idPost, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .WithName("ListarComentariosPost")
        .WithSummary("Lista os comentários de uma review")
        .Produces<IReadOnlyList<ComentarioResponseDto>>(StatusCodes.Status200OK);

        group.MapPost("/{idPost:int}/comentarios", async (
            int idPost,
            CriarComentarioRequestDto request,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AdicionarComentarioAsync(idPost, idUsuario, request, cancellationToken);
            return TypedResults.Created($"/api/v1/posts/{idPost}/comentarios/{resposta.IdComentario}", resposta);
        })
        .RequireAuthorization()
        .WithName("CriarComentarioPost")
        .WithSummary("Adiciona um novo comentário (com suporte a marcação de spoiler) em uma review")
        .Produces<ComentarioResponseDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/comentarios/{idComentario:int}/curtir", async (
            int idComentario,
            ClaimsPrincipal user,
            IPostService service,
            CancellationToken cancellationToken) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AlternarCurtidaComentarioAsync(idComentario, idUsuario, cancellationToken);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization()
        .WithName("AlternarCurtidaComentario")
        .WithSummary("Curte ou descurte um comentário")
        .Produces<CurtidaResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
