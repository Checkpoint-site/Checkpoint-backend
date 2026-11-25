using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Estantes.Respost;
using Domain.Dtos.Loggers.Commands;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Estantes;

public abstract class RegraEstanteCatalogo
{
    public abstract TipoEstante TipoEstante { get; }
    public abstract string ChaveExibicao { get; }
    public abstract int LimiteMaximoJogos { get; }

    public virtual void ValidarAdicaoJogo(EstanteCatalogoRespostDto estante, JogoCatalogoRespostDto jogo)
    {
        if (estante.Jogos.Contains(jogo.IdJogo))
        {
            throw new ConflictException("Este jogo já está presente nesta estante.");
        }

        if (estante.Jogos.Count >= LimiteMaximoJogos)
        {
            throw new ValidationException($"Limite máximo de {LimiteMaximoJogos} jogos atingido para a estante '{ChaveExibicao}'.");
        }
    }

    public abstract ImpactoEngajamentoEstanteCommandDto CriarComandoImpactoLoggers(
        int idUsuario,
        JogoCatalogoRespostDto jogo,
        bool adicionando);

    protected static int ObterDelta(bool adicionando) => adicionando ? 1 : -1;
}
