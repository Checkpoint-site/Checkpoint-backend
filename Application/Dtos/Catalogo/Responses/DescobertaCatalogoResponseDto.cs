namespace Application.Dtos.Catalogo.Responses;

public record DescobertaCatalogoResponseDto
{
    public required string OrigemDados { get; init; }
    public required string AlgoritmoAplicado { get; init; }
    public required int TotalResultados { get; init; }
    public required IReadOnlyList<JogoResumoResponseDto> Itens { get; init; }
}
