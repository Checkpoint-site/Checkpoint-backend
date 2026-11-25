namespace Application.Dtos.Catalogo.Responses;

public record BuscaCatalogoResponseDto
{
    public required string TipoBusca { get; init; }
    public required string Termo { get; init; }
    public required string AlgoritmoAplicado { get; init; }
    public bool PersonalizadoParaUsuario { get; init; }
    public int TotalResultados { get; init; }
    public required IReadOnlyList<JogoResumoResponseDto> Itens { get; init; }
}
