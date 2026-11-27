using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Utils;

public static class RegrasModeracaoUtil
{
    public static bool UsuarioEstaSuspenso(Usuario usuario, DateTime? referenciaUtc = null) =>
        usuario.SuspensoAte.HasValue && usuario.SuspensoAte.Value > (referenciaUtc ?? DateTime.UtcNow);

    public static bool SolicitacaoEstaAberta(StatusSolicitacaoModerador status) =>
        status is StatusSolicitacaoModerador.Pendente or StatusSolicitacaoModerador.AguardandoSegundaAprovacao;

    public static bool AprovarSolicitacao(
        SolicitacaoModerador solicitacao,
        int idAvaliador,
        TipoPerfilUsuario perfilAvaliador,
        DateTime? agoraUtc = null)
    {
        if (perfilAvaliador is not (TipoPerfilUsuario.Moderador or TipoPerfilUsuario.Admin))
        {
            throw new ForbiddenException("Apenas moderadores ou administradores podem aprovar solicitações.");
        }

        if (idAvaliador == solicitacao.IdUsuarioSolicitante)
        {
            throw new ValidationException("Você não pode aprovar a própria solicitação.");
        }

        if (!SolicitacaoEstaAberta(solicitacao.Status))
        {
            throw new ValidationException($"Não é possível aprovar uma solicitação com status '{solicitacao.Status}'.");
        }

        var agora = agoraUtc ?? DateTime.UtcNow;

        if (perfilAvaliador == TipoPerfilUsuario.Admin)
        {
            if (solicitacao.IdPrimeiroAprovador is null)
            {
                solicitacao.IdPrimeiroAprovador = idAvaliador;
                solicitacao.DataPrimeiraAprovacao = agora;
            }
            else
            {
                solicitacao.IdSegundoAprovador = idAvaliador;
                solicitacao.DataSegundaAprovacao = agora;
            }

            solicitacao.Status = StatusSolicitacaoModerador.Aprovada;
            solicitacao.DataAtualizacao = agora;
            return true;
        }

        if (solicitacao.Status == StatusSolicitacaoModerador.Pendente)
        {
            solicitacao.IdPrimeiroAprovador = idAvaliador;
            solicitacao.DataPrimeiraAprovacao = agora;
            solicitacao.Status = StatusSolicitacaoModerador.AguardandoSegundaAprovacao;
            solicitacao.DataAtualizacao = agora;
            return false;
        }

        if (solicitacao.IdPrimeiroAprovador == idAvaliador)
        {
            throw new ValidationException("Você já aprovou esta solicitação. É necessária a aprovação de outro moderador.");
        }

        solicitacao.IdSegundoAprovador = idAvaliador;
        solicitacao.DataSegundaAprovacao = agora;
        solicitacao.Status = StatusSolicitacaoModerador.Aprovada;
        solicitacao.DataAtualizacao = agora;
        return true;
    }

    public static void RecusarSolicitacao(
        SolicitacaoModerador solicitacao,
        int idAvaliador,
        TipoPerfilUsuario perfilAvaliador,
        string motivoRecusa,
        DateTime? agoraUtc = null)
    {
        if (perfilAvaliador is not (TipoPerfilUsuario.Moderador or TipoPerfilUsuario.Admin))
        {
            throw new ForbiddenException("Apenas moderadores ou administradores podem recusar solicitações.");
        }

        if (idAvaliador == solicitacao.IdUsuarioSolicitante)
        {
            throw new ValidationException("Você não pode recusar a própria solicitação; utilize a opção de cancelamento.");
        }

        if (!SolicitacaoEstaAberta(solicitacao.Status))
        {
            throw new ValidationException($"Não é possível recusar uma solicitação com status '{solicitacao.Status}'.");
        }

        if (string.IsNullOrWhiteSpace(motivoRecusa))
        {
            throw new ValidationException("O motivo da recusa é obrigatório.");
        }

        var agora = agoraUtc ?? DateTime.UtcNow;
        solicitacao.Status = StatusSolicitacaoModerador.Recusada;
        solicitacao.IdAvaliadorRecusa = idAvaliador;
        solicitacao.MotivoRecusa = motivoRecusa.Trim();
        solicitacao.DataAtualizacao = agora;
    }

    public static void CancelarSolicitacao(
        SolicitacaoModerador solicitacao,
        int idUsuario,
        DateTime? agoraUtc = null)
    {
        if (idUsuario != solicitacao.IdUsuarioSolicitante)
        {
            throw new ForbiddenException("Apenas o solicitante pode cancelar esta solicitação.");
        }

        if (solicitacao.Status != StatusSolicitacaoModerador.Pendente)
        {
            throw new ValidationException("Apenas solicitações pendentes podem ser canceladas.");
        }

        solicitacao.Status = StatusSolicitacaoModerador.Cancelada;
        solicitacao.DataAtualizacao = agoraUtc ?? DateTime.UtcNow;
    }

    public static void AlterarMarcaSpoilerPost(Post post, bool temSpoiler)
    {
        if (post.PostDeletado)
        {
            throw new ValidationException("Não é possível alterar a marcação de spoiler de um post excluído.");
        }

        post.TemSpoiler = temSpoiler;
    }

    public static void AlterarMarcaSpoilerComentario(Comentario comentario, bool temSpoiler)
    {
        if (comentario.ComentarioDeletado)
        {
            throw new ValidationException("Não é possível alterar a marcação de spoiler de um comentário excluído.");
        }

        comentario.TemSpoiler = temSpoiler;
    }
}
