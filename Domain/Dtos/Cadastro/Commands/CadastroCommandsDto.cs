using Domain.Enum;

namespace Domain.Dtos.Cadastro.Commands;

public record CadastroCommandsDto
{
    public required string Nome { get; init; }
    public required string Email { get; init; }
    public required string Senha { get; init; }
    public TipoPerfilUsuario TipoPerfil { get; init; } = TipoPerfilUsuario.Regular;
    public string? FotoPerfil { get; init; }
}
