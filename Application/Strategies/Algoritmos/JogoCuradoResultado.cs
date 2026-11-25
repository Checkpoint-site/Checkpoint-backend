using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;

namespace Application.Strategies.Algoritmos;

public record JogoCuradoResultado(
    JogoCatalogoRespostDto Jogo,
    double PontuacaoCuradoria,
    double MediaFogueiras,
    OfertaJogoSteamDto? Oferta = null
);
