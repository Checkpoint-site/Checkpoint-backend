using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;

namespace Domain.Dtos.Catalogo;

public record ItemPoolFallbackSteamDto
{
    public required JogoCatalogoRespostDto Jogo { get; init; }
    public OfertaJogoSteamDto? Oferta { get; init; }
    public double PesoRelevanciaFonte { get; init; } = 1.0;
}
