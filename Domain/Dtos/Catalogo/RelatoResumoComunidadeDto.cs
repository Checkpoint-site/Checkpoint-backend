namespace Domain.Dtos.Catalogo;

public record RelatoResumoComunidadeDto
{
    public required int IdPost { get; init; }
    public required int IdJogo { get; init; }
    public int SteamAppId { get; init; }
    public required string NomeJogo { get; init; }
    public required string Titulo { get; init; }
    public required double Fogueiras { get; init; }
    public int QuantidadeCurtidas { get; init; }
    public bool TemSpoiler { get; init; } = false;
    public required DateTime DataCriacao { get; init; }
}
