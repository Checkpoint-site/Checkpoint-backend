using Application.Strategies.Busca;
using Domain.Enum;

namespace Application.Interfaces;

public interface IBuscaCatalogoStrategyFactory
{
    EstrategiaBuscaCatalogo Criar(TipoBuscaCatalogo tipo);
    EstrategiaBuscaCatalogo Criar(string? tipo);
}
