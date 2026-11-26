using System.Security.Claims;
using Api.Extension;
using Application.Dtos.Moderacao.Requests;
using Application.Dtos.Moderacao.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Moderacao;

public static class ModeracaoEndpoint
{
    public static void MapModeracaoEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/moderacao")
            .WithTags("Moderação");

        group.MapPost("/solicitacoes", async (
            CriarSolicitacaoModeradorRequestDto request,
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.SolicitarAsync(idUsuario, request);
            return TypedResults.Created($"/api/v1/moderacao/solicitacoes/{resposta.Id}", resposta);
        })
        .RequireAuthorization("UsuarioRegularPolicy")
        .WithName("CriarSolicitacaoModerador")
        .WithSummary("Solicita promoção para o perfil de moderador")
        .WithDescription("Permite que um usuário regular abra uma solicitação para se tornar moderador.")
        .Produces<SolicitacaoModeradorResponseDto>(201)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(409);

        group.MapGet("/solicitacoes/minha", async (
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.ObterMinhaSolicitacaoAsync(idUsuario);
            return resposta is null ? Results.NoContent() : TypedResults.Ok(resposta);
        })
        .RequireAuthorization("UsuarioRegularPolicy")
        .WithName("ObterMinhaSolicitacaoModerador")
        .WithSummary("Consulta a solicitação de moderação mais recente do usuário autenticado")
        .Produces<SolicitacaoModeradorResponseDto>(200)
        .Produces(204)
        .ProducesProblem(401);

        group.MapPost("/solicitacoes/{id:int}/cancelar", async (
            int id,
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idUsuario = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.CancelarAsync(id, idUsuario);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization("UsuarioRegularPolicy")
        .WithName("CancelarSolicitacaoModerador")
        .WithSummary("Cancela uma solicitação pendente do próprio usuário")
        .Produces<SolicitacaoModeradorResponseDto>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);

        group.MapGet("/solicitacoes", async (
            string? status,
            ISolicitacaoModeradorService service) =>
        {
            var resposta = await service.ListarAsync(status);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization("ModeradorPolicy")
        .WithName("ListarSolicitacoesModerador")
        .WithSummary("Lista solicitações de moderação")
        .WithDescription("Retorna por padrão as solicitações abertas (Pendente e AguardandoSegundaAprovacao), podendo filtrar por status.")
        .Produces<IReadOnlyList<SolicitacaoModeradorResponseDto>>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403);

        group.MapPost("/solicitacoes/{id:int}/aprovar", async (
            int id,
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idAvaliador = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AprovarAsync(id, idAvaliador);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization("ModeradorPolicy")
        .WithName("AprovarSolicitacaoModerador")
        .WithSummary("Aprova uma solicitação de moderação")
        .WithDescription("Admin aprova imediatamente em 1 etapa; Moderador exige 2 aprovações de moderadores distintos.")
        .Produces<SolicitacaoModeradorResponseDto>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);

        group.MapPost("/solicitacoes/{id:int}/recusar", async (
            int id,
            RecusarSolicitacaoModeradorRequestDto request,
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idAvaliador = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.RecusarAsync(id, idAvaliador, request);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization("ModeradorPolicy")
        .WithName("RecusarSolicitacaoModerador")
        .WithSummary("Recusa uma solicitação de moderação com motivo obrigatório")
        .Produces<SolicitacaoModeradorResponseDto>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);

        group.MapPost("/usuarios/{usuarioId:int}/rebaixar", async (
            int usuarioId,
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idAdmin = user.ObterIdUsuarioObrigatorio();
            await service.RebaixarModeradorAsync(idAdmin, usuarioId);
            return TypedResults.NoContent();
        })
        .RequireAuthorization("AdminPolicy")
        .WithName("RebaixarModerador")
        .WithSummary("Rebaixa um moderador para usuário regular (exclusivo Admin)")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);

        group.MapPatch("/posts/{idPost:int}/spoiler", async (
            int idPost,
            AlterarSpoilerPostRequestDto request,
            ClaimsPrincipal user,
            ISolicitacaoModeradorService service) =>
        {
            var idAvaliador = user.ObterIdUsuarioObrigatorio();
            var resposta = await service.AlterarSpoilerPostAsync(idPost, idAvaliador, request);
            return TypedResults.Ok(resposta);
        })
        .RequireAuthorization("ModeradorPolicy")
        .WithName("AlterarSpoilerPost")
        .WithSummary("Marca ou desmarca aviso de spoiler em um post (Moderador/Admin)")
        .WithDescription("Permite que moderadores ou administradores ativem ou removam a censura de spoiler de um post.")
        .Produces<PostSpoilerResponseDto>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404);
    }
}
