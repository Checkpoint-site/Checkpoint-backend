namespace Domain.Dtos.Perfis.Commands;

public record AlterarSuspensaoUsuarioCommandDto
{
    public required int IdModerador { get; init; }
    public required int IdUsuarioAlvo { get; init; }
    public DateTime? SuspensoAte { get; init; }
    public string? Motivo { get; init; }
    public required int NovoTotalAcoesModeracao { get; init; }
}
