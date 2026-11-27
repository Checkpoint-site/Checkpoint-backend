using Domain.Enum;

namespace Domain.Dtos.Login.Respost;

public record LoginRespostDto
{
    public required int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public required string Email { get; init; }
    public required string Senha { get; init; }
    public TipoPerfilUsuario TipoPerfil { get; init; } = TipoPerfilUsuario.Regular;
    public DateTime? SuspensoAte { get; init; }
    public string? FotoPerfil { get; init; }
}
