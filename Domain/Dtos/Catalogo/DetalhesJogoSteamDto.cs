using Domain.Enum;

namespace Domain.Dtos.Catalogo;

public record DetalhesJogoSteamDto
{
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public required string DescricaoCurta { get; init; }
    public required string ImagemCapaUrl { get; init; }
    public required string Desenvolvedora { get; init; }
    public required string Publicadora { get; init; }
    public required IReadOnlyList<Generos> Generos { get; init; }
    public string? DataLancamentoTexto { get; init; }
    public int? AnoLancamento { get; init; }
    public int? NotaMetacritic { get; init; }
    public int TotalRecomendacoes { get; init; }
    public bool Gratuito { get; init; }
    public bool EmBreve { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public int PercentualDesconto { get; init; }
}
