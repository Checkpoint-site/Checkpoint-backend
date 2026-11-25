using Application.Dtos.Catalogo.Responses;

namespace Application.Dtos.Estantes.Responses;

public record EstanteFavoritosResponseDto
{
    public int IdCatalogo { get; init; }
    public bool ExibirNoTopoPerfil { get; init; }
    public int? JogoDestaquePerfilId { get; init; }
    public string? ObservacaoDestaque { get; init; }
    public int TotalJogos { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public required IReadOnlyList<JogoResumoResponseDto> Jogos { get; init; }
}
