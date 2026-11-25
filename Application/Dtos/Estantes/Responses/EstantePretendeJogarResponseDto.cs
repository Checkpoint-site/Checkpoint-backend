using Application.Dtos.Catalogo.Responses;

namespace Application.Dtos.Estantes.Responses;

public record EstantePretendeJogarResponseDto
{
    public int IdCatalogo { get; init; }
    public int NivelPrioridade { get; init; }
    public bool NotificarEmPromocao { get; init; }
    public int? MetaAnoConclusao { get; init; }
    public int TotalJogos { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public required IReadOnlyList<JogoResumoResponseDto> Jogos { get; init; }
}
