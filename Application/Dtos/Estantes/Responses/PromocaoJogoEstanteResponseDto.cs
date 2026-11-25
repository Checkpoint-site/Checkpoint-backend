namespace Application.Dtos.Estantes.Responses;

public record PromocaoJogoEstanteResponseDto
{
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public required string Imagem { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public required int PercentualDesconto { get; init; }
    public required DateTime ConsultadoEm { get; init; }
}
