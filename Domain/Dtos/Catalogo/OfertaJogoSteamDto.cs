namespace Domain.Dtos.Catalogo;

public record OfertaJogoSteamDto
{
    public required int SteamAppId { get; init; }
    public bool Gratuito { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public int PercentualDesconto { get; init; }
    public DateTime ConsultadoEm { get; init; } = DateTime.UtcNow;
}
