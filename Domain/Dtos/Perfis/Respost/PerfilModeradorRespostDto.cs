namespace Domain.Dtos.Perfis.Respost;

public record PerfilModeradorRespostDto : PerfilUsuarioRespostDto
{
    public required string CodigoCredencial { get; init; }
    public required string SetorModeracao { get; init; }
    public int TotalAcoesModeracao { get; init; } = 0;
}
