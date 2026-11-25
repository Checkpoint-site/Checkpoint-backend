namespace Domain.Dtos.Estantes.Respost;

public abstract record EstanteCatalogoRespostDto
{
    public int IdCatalogo { get; init; }
    public required int IdUsuario { get; init; }
    public required IReadOnlyList<int> Jogos { get; init; }
    public DateTime DataAtualizacao { get; init; }
}
