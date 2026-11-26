namespace Application.Dtos.Moderacao.Responses;

public record SolicitacaoModeradorResponseDto
{
    public required int Id { get; init; }
    public required int IdUsuarioSolicitante { get; init; }
    public required string NomeSolicitante { get; init; }
    public required string EmailSolicitante { get; init; }
    public required string Motivo { get; init; }
    public required string Status { get; init; }
    public int? IdPrimeiroAprovador { get; init; }
    public string? NomePrimeiroAprovador { get; init; }
    public DateTime? DataPrimeiraAprovacao { get; init; }
    public int? IdSegundoAprovador { get; init; }
    public string? NomeSegundoAprovador { get; init; }
    public DateTime? DataSegundaAprovacao { get; init; }
    public int? IdAvaliadorRecusa { get; init; }
    public string? MotivoRecusa { get; init; }
    public required DateTime DataCriacao { get; init; }
    public required DateTime DataAtualizacao { get; init; }
}
