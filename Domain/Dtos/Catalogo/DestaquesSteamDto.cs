namespace Domain.Dtos.Catalogo;

public record DestaquesSteamDto
{
    public required IReadOnlyList<ItemDestaqueSteamDto> MaisVendidos { get; init; }
    public required IReadOnlyList<ItemDestaqueSteamDto> Lancamentos { get; init; }
    public required IReadOnlyList<ItemDestaqueSteamDto> OfertasEspeciais { get; init; }
    public required IReadOnlyList<ItemDestaqueSteamDto> EmBreve { get; init; }
}
