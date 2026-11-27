namespace Application.Dtos.Catalogo.Responses;

public record ItemVitrineResponseDto
{
    public int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public required string ImagemCapaUrl { get; init; }
    public string Desenvolvedora { get; init; } = "Steam";
    public int? AnoLancamento { get; init; }
    public IReadOnlyList<string> Generos { get; init; } = [];
    public bool Gratuito { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public int PercentualDesconto { get; init; }
    public double PontuacaoCuradoria { get; init; }
    public double MediaFogueiras { get; init; }
    public int TotalAvaliacoes { get; init; }
}
