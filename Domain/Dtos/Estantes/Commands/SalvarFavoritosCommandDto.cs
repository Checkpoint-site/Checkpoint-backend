namespace Domain.Dtos.Estantes.Commands;

public record SalvarFavoritosCommandDto
{
    public required int IdUsuario { get; init; }
    public required IReadOnlyList<int> Jogos { get; init; }
    public int? JogoDestaquePerfilId { get; init; }
    public bool ExibirNoTopoPerfil { get; init; } = true;
    public string? ObservacaoDestaque { get; init; }
}
