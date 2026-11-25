namespace Application.Dtos.Catalogo.Responses;

public record JogoResumoResponseDto
{
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public required string Imagem { get; init; }
    public required string Desenvolvedora { get; init; }
    public required string Publicadora { get; init; }
    public int? AnoLancamento { get; init; }
    public int? NotaMetacritic { get; init; }
    public bool Gratuito { get; init; }
    public bool EmBreve { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public int PercentualDesconto { get; init; }
    public double PontuacaoCuradoria { get; init; }
    public double MediaFogueiras { get; init; }
    public required IReadOnlyList<string> Generos { get; init; }
}
