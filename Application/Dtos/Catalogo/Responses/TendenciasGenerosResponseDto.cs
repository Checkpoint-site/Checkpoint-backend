namespace Application.Dtos.Catalogo.Responses;

public record TendenciasGenerosResponseDto
{
    public required IReadOnlyList<TendenciaGeneroItemResponseDto> Generos { get; init; }
}
