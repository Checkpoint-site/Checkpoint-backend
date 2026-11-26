using Domain.Enum;
using Domain.Exceptions.Common;

namespace Domain.Model;

public class SolicitacaoModerador
{
    public int Id { get; set; }
    public int IdUsuarioSolicitante { get; set; }
    public Usuario? UsuarioSolicitante { get; set; }
    public required string Motivo { get; set; }
    public StatusSolicitacaoModerador Status { get; set; } = StatusSolicitacaoModerador.Pendente;
    public int? IdPrimeiroAprovador { get; set; }
    public Usuario? PrimeiroAprovador { get; set; }
    public DateTime? DataPrimeiraAprovacao { get; set; }
    public int? IdSegundoAprovador { get; set; }
    public Usuario? SegundoAprovador { get; set; }
    public DateTime? DataSegundaAprovacao { get; set; }
    public int? IdAvaliadorRecusa { get; set; }
    public Usuario? AvaliadorRecusa { get; set; }
    public string? MotivoRecusa { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    public bool EstaAberta() =>
        Status is StatusSolicitacaoModerador.Pendente or StatusSolicitacaoModerador.AguardandoSegundaAprovacao;

    /// <summary>
    /// Registra uma aprovação na solicitação.
    /// Admin aprova imediatamente (1 aprovação). Moderador exige 2 aprovações de moderadores distintos.
    /// Retorna true quando a solicitação atinge o estado final Aprovada.
    /// </summary>
    public bool Aprovar(int idAvaliador, TipoPerfilUsuario perfilAvaliador, DateTime? agoraUtc = null)
    {
        if (perfilAvaliador is not (TipoPerfilUsuario.Moderador or TipoPerfilUsuario.Admin))
        {
            throw new ForbiddenException("Apenas moderadores ou administradores podem aprovar solicitações.");
        }

        if (idAvaliador == IdUsuarioSolicitante)
        {
            throw new ValidationException("Você não pode aprovar a própria solicitação.");
        }

        if (!EstaAberta())
        {
            throw new ValidationException($"Não é possível aprovar uma solicitação com status '{Status}'.");
        }

        var agora = agoraUtc ?? DateTime.UtcNow;

        if (perfilAvaliador == TipoPerfilUsuario.Admin)
        {
            if (IdPrimeiroAprovador is null)
            {
                IdPrimeiroAprovador = idAvaliador;
                DataPrimeiraAprovacao = agora;
            }
            else
            {
                IdSegundoAprovador = idAvaliador;
                DataSegundaAprovacao = agora;
            }

            Status = StatusSolicitacaoModerador.Aprovada;
            DataAtualizacao = agora;
            return true;
        }

        if (Status == StatusSolicitacaoModerador.Pendente)
        {
            IdPrimeiroAprovador = idAvaliador;
            DataPrimeiraAprovacao = agora;
            Status = StatusSolicitacaoModerador.AguardandoSegundaAprovacao;
            DataAtualizacao = agora;
            return false;
        }

        if (IdPrimeiroAprovador == idAvaliador)
        {
            throw new ValidationException("Você já aprovou esta solicitação. É necessária a aprovação de outro moderador.");
        }

        IdSegundoAprovador = idAvaliador;
        DataSegundaAprovacao = agora;
        Status = StatusSolicitacaoModerador.Aprovada;
        DataAtualizacao = agora;
        return true;
    }

    public void Recusar(int idAvaliador, TipoPerfilUsuario perfilAvaliador, string motivoRecusa, DateTime? agoraUtc = null)
    {
        if (perfilAvaliador is not (TipoPerfilUsuario.Moderador or TipoPerfilUsuario.Admin))
        {
            throw new ForbiddenException("Apenas moderadores ou administradores podem recusar solicitações.");
        }

        if (idAvaliador == IdUsuarioSolicitante)
        {
            throw new ValidationException("Você não pode recusar a própria solicitação; utilize a opção de cancelamento.");
        }

        if (!EstaAberta())
        {
            throw new ValidationException($"Não é possível recusar uma solicitação com status '{Status}'.");
        }

        if (string.IsNullOrWhiteSpace(motivoRecusa))
        {
            throw new ValidationException("O motivo da recusa é obrigatório.");
        }

        var agora = agoraUtc ?? DateTime.UtcNow;
        Status = StatusSolicitacaoModerador.Recusada;
        IdAvaliadorRecusa = idAvaliador;
        MotivoRecusa = motivoRecusa.Trim();
        DataAtualizacao = agora;
    }

    public void Cancelar(int idUsuario, DateTime? agoraUtc = null)
    {
        if (idUsuario != IdUsuarioSolicitante)
        {
            throw new ForbiddenException("Apenas o solicitante pode cancelar esta solicitação.");
        }

        if (Status != StatusSolicitacaoModerador.Pendente)
        {
            throw new ValidationException("Apenas solicitações pendentes podem ser canceladas.");
        }

        Status = StatusSolicitacaoModerador.Cancelada;
        DataAtualizacao = agoraUtc ?? DateTime.UtcNow;
    }
}
