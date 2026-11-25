namespace Application.Dtos.Catalogo.Responses;

public record JogoDetalhesResponseDto
{
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public required string Descricao { get; init; }
    public required string Imagem { get; init; }
    public required string Desenvolvedora { get; init; }
    public required string Publicadora { get; init; }
    public required IReadOnlyList<string> Generos { get; init; }
    public string? DataLancamentoTexto { get; init; }
    public int? AnoLancamento { get; init; }
    public int? NotaMetacritic { get; init; }
    public int TotalRecomendacoes { get; init; }
    public bool Gratuito { get; init; }
    public bool EmBreve { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public int PercentualDesconto { get; init; }
    public DateTime SincronizadoEm { get; init; }
    public required JogoMetricasLoggerResponseDto EngajamentoComunidade { get; init; }
}
