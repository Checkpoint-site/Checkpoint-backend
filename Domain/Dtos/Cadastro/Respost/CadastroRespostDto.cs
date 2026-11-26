using Domain.Enum;

namespace Domain.Dtos.Cadastro.Respost;

public record CadastroRespostDto
{
    public required int Id { get; init; }
    public required string Nome { get; init; }
    public required string Email { get; init; }
    public TipoPerfilUsuario TipoPerfil { get; init; } = TipoPerfilUsuario.Regular;
}
