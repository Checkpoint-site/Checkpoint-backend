using Application.Dtos.Moderacao.Requests;
using Application.Dtos.Moderacao.Responses;
using Application.Interfaces;
using Application.Utils;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Model;

namespace Application.Services;

public class SolicitacaoModeradorService(ISolicitacaoModeradorRepository repository) : ISolicitacaoModeradorService
{
    public async Task<SolicitacaoModeradorResponseDto> SolicitarAsync(
        int idUsuarioSolicitante,
        CriarSolicitacaoModeradorRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 10)
        {
            throw new ValidationException("O motivo da solicitação deve ter pelo menos 10 caracteres.");
        }

        var usuario = await repository.ObterUsuarioPorIdAsync(idUsuarioSolicitante)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (usuario.ContaDeletada)
        {
            throw new ForbiddenException("Contas excluídas não podem solicitar moderação.");
        }

        if (RegrasModeracaoUtil.UsuarioEstaSuspenso(usuario))
        {
            throw new ForbiddenException("Usuários suspensos não podem solicitar acesso de moderador.");
        }

        if (usuario.TipoPerfil != TipoPerfilUsuario.Regular)
        {
            throw new ConflictException("Você já possui permissões de moderação ou administração.");
        }

        var jaPossuiAberta = await repository.ExisteSolicitacaoAbertaAsync(idUsuarioSolicitante);
        if (jaPossuiAberta)
        {
            throw new ConflictException("Você já possui uma solicitação de moderação em andamento.");
        }

        var agora = DateTime.UtcNow;
        var novaSolicitacao = new SolicitacaoModerador
        {
            IdUsuarioSolicitante = idUsuarioSolicitante,
            UsuarioSolicitante = usuario,
            Motivo = request.Motivo.Trim(),
            Status = StatusSolicitacaoModerador.Pendente,
            DataCriacao = agora,
            DataAtualizacao = agora
        };

        var criada = await repository.CriarAsync(novaSolicitacao);
        return MapearResposta(criada);
    }

    public async Task<SolicitacaoModeradorResponseDto?> ObterMinhaSolicitacaoAsync(int idUsuarioSolicitante)
    {
        var solicitacao = await repository.ObterUltimaPorUsuarioAsync(idUsuarioSolicitante);
        return solicitacao is null ? null : MapearResposta(solicitacao);
    }

    public async Task<IReadOnlyList<SolicitacaoModeradorResponseDto>> ListarAsync(string? status = null)
    {
        StatusSolicitacaoModerador? filtroStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusSolicitacaoModerador>(status.Trim(), ignoreCase: true, out var parsed)
                || !Enum.IsDefined(parsed))
            {
                throw new ValidationException($"Status de solicitação '{status}' é inválido.");
            }

            filtroStatus = parsed;
        }

        var solicitacoes = await repository.ListarAsync(filtroStatus);
        return [.. solicitacoes.Select(MapearResposta)];
    }

    public async Task<SolicitacaoModeradorResponseDto> AprovarAsync(int idSolicitacao, int idAvaliador)
    {
        var avaliador = await repository.ObterUsuarioPorIdAsync(idAvaliador)
            ?? throw new NotFoundException("Avaliador não encontrado.");

        if (avaliador.ContaDeletada || RegrasModeracaoUtil.UsuarioEstaSuspenso(avaliador))
        {
            throw new ForbiddenException("Sua conta não tem permissão ativa para avaliar solicitações.");
        }

        var solicitacao = await repository.ObterPorIdAsync(idSolicitacao)
            ?? throw new NotFoundException("Solicitação de moderação não encontrada.");

        var aprovadaDefinitivamente = RegrasModeracaoUtil.AprovarSolicitacao(solicitacao, avaliador.Id, avaliador.TipoPerfil);

        if (aprovadaDefinitivamente)
        {
            var solicitante = solicitacao.UsuarioSolicitante
                ?? await repository.ObterUsuarioPorIdAsync(solicitacao.IdUsuarioSolicitante)
                ?? throw new NotFoundException("Usuário solicitante não encontrado.");

            solicitante.TipoPerfil = TipoPerfilUsuario.Moderador;
        }

        await repository.SalvarAlteracoesAsync();

        var atualizada = await repository.ObterPorIdAsync(solicitacao.Id) ?? solicitacao;
        return MapearResposta(atualizada);
    }

    public async Task<SolicitacaoModeradorResponseDto> RecusarAsync(
        int idSolicitacao,
        int idAvaliador,
        RecusarSolicitacaoModeradorRequestDto request)
    {
        var avaliador = await repository.ObterUsuarioPorIdAsync(idAvaliador)
            ?? throw new NotFoundException("Avaliador não encontrado.");

        if (avaliador.ContaDeletada || RegrasModeracaoUtil.UsuarioEstaSuspenso(avaliador))
        {
            throw new ForbiddenException("Sua conta não tem permissão ativa para avaliar solicitações.");
        }

        var solicitacao = await repository.ObterPorIdAsync(idSolicitacao)
            ?? throw new NotFoundException("Solicitação de moderação não encontrada.");

        RegrasModeracaoUtil.RecusarSolicitacao(solicitacao, avaliador.Id, avaliador.TipoPerfil, request.Motivo);
        await repository.SalvarAlteracoesAsync();

        var atualizada = await repository.ObterPorIdAsync(solicitacao.Id) ?? solicitacao;
        return MapearResposta(atualizada);
    }

    public async Task<SolicitacaoModeradorResponseDto> CancelarAsync(int idSolicitacao, int idUsuarioSolicitante)
    {
        var solicitacao = await repository.ObterPorIdAsync(idSolicitacao)
            ?? throw new NotFoundException("Solicitação de moderação não encontrada.");

        RegrasModeracaoUtil.CancelarSolicitacao(solicitacao, idUsuarioSolicitante);
        await repository.SalvarAlteracoesAsync();

        return MapearResposta(solicitacao);
    }

    public async Task RebaixarModeradorAsync(int idAdmin, int idUsuarioAlvo)
    {
        var admin = await repository.ObterUsuarioPorIdAsync(idAdmin)
            ?? throw new NotFoundException("Administrador não encontrado.");

        if (admin.TipoPerfil != TipoPerfilUsuario.Admin || admin.ContaDeletada || RegrasModeracaoUtil.UsuarioEstaSuspenso(admin))
        {
            throw new ForbiddenException("Apenas administradores ativos podem rebaixar moderadores.");
        }

        if (idAdmin == idUsuarioAlvo)
        {
            throw new ValidationException("Um administrador não pode rebaixar a própria conta.");
        }

        var alvo = await repository.ObterUsuarioPorIdAsync(idUsuarioAlvo)
            ?? throw new NotFoundException("Usuário alvo não encontrado.");

        if (alvo.TipoPerfil == TipoPerfilUsuario.Admin)
        {
            throw new ForbiddenException("Não é permitido rebaixar outro administrador.");
        }

        if (alvo.TipoPerfil != TipoPerfilUsuario.Moderador)
        {
            throw new ValidationException("O usuário informado não possui perfil de moderador.");
        }

        alvo.TipoPerfil = TipoPerfilUsuario.Regular;
        await repository.SalvarAlteracoesAsync();
    }

    public async Task<PostSpoilerResponseDto> AlterarSpoilerPostAsync(
        int idPost,
        int idAvaliador,
        AlterarSpoilerPostRequestDto request)
    {
        var avaliador = await repository.ObterUsuarioPorIdAsync(idAvaliador)
            ?? throw new NotFoundException("Avaliador não encontrado.");

        if (avaliador.TipoPerfil is not (TipoPerfilUsuario.Moderador or TipoPerfilUsuario.Admin)
            || avaliador.ContaDeletada
            || RegrasModeracaoUtil.UsuarioEstaSuspenso(avaliador))
        {
            throw new ForbiddenException("Apenas moderadores ou administradores ativos podem alterar marcações de spoiler.");
        }

        var post = await repository.ObterPostPorIdAsync(idPost)
            ?? throw new NotFoundException("Post não encontrado.");

        RegrasModeracaoUtil.AlterarMarcaSpoilerPost(post, request.TemSpoiler);
        await repository.SalvarAlteracoesAsync();

        return new PostSpoilerResponseDto
        {
            IdPost = post.IdPost,
            Titulo = post.Titulo,
            TemSpoiler = post.TemSpoiler
        };
    }

    private static SolicitacaoModeradorResponseDto MapearResposta(SolicitacaoModerador solicitacao) => new()
    {
        Id = solicitacao.Id,
        IdUsuarioSolicitante = solicitacao.IdUsuarioSolicitante,
        NomeSolicitante = solicitacao.UsuarioSolicitante?.Nome ?? string.Empty,
        EmailSolicitante = solicitacao.UsuarioSolicitante?.Email ?? string.Empty,
        Motivo = solicitacao.Motivo,
        Status = solicitacao.Status.ToString(),
        IdPrimeiroAprovador = solicitacao.IdPrimeiroAprovador,
        NomePrimeiroAprovador = solicitacao.PrimeiroAprovador?.Nome,
        DataPrimeiraAprovacao = solicitacao.DataPrimeiraAprovacao,
        IdSegundoAprovador = solicitacao.IdSegundoAprovador,
        NomeSegundoAprovador = solicitacao.SegundoAprovador?.Nome,
        DataSegundaAprovacao = solicitacao.DataSegundaAprovacao,
        IdAvaliadorRecusa = solicitacao.IdAvaliadorRecusa,
        MotivoRecusa = solicitacao.MotivoRecusa,
        DataCriacao = solicitacao.DataCriacao,
        DataAtualizacao = solicitacao.DataAtualizacao
    };
}
