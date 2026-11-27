using Domain.Enum;

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
}
