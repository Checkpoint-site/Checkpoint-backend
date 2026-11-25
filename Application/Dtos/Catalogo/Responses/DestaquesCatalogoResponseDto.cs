namespace Application.Dtos.Catalogo.Responses;

public record DestaquesCatalogoResponseDto
{
    public required string AlgoritmoAplicado { get; init; }
    public required IReadOnlyList<ItemVitrineResponseDto> MaisVendidos { get; init; }
    public required IReadOnlyList<ItemVitrineResponseDto> Lancamentos { get; init; }
    public required IReadOnlyList<ItemVitrineResponseDto> OfertasEspeciais { get; init; }
    public required IReadOnlyList<ItemVitrineResponseDto> EmBreve { get; init; }
}
