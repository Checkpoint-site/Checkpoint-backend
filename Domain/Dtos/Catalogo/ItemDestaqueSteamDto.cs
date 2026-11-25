namespace Domain.Dtos.Catalogo;

public record ItemDestaqueSteamDto
{
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public required string ImagemCapaUrl { get; init; }
    public bool Gratuito { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public int PercentualDesconto { get; init; }
}
