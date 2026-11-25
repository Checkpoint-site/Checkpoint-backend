namespace Domain.Dtos.Estantes.Respost;

public record FavoritosRespostDto : EstanteCatalogoRespostDto
{
    public int? JogoDestaquePerfilId { get; init; }
    public bool ExibirNoTopoPerfil { get; init; } = true;
    public string? ObservacaoDestaque { get; init; }
}
