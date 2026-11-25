using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Strategies.Estantes;

public abstract class RegraEstanteCatalogo
{
    public abstract string TipoEstante { get; }
    public abstract int LimiteMaximoJogos { get; }

    public virtual void ValidarAdicaoJogo(Catalogo estante, Jogo jogo)
    {
        if (estante.Jogos.Contains(jogo.IdJogo))
        {
            throw new ConflictException("Este jogo já está presente nesta estante.");
        }

        if (estante.Jogos.Count >= LimiteMaximoJogos)
        {
            throw new ValidationException($"Limite máximo de {LimiteMaximoJogos} jogos atingido para a estante '{TipoEstante}'.");
        }
    }

    public abstract void AtualizarLoggers(
        JogoLogger jogoLogger,
        IEnumerable<GeneroLogger> generoLoggers,
        UsuarioLogger usuarioLogger,
        bool adicionando);

    protected static ulong AjustarContador(ulong atual, bool adicionando)
    {
        if (adicionando)
        {
            return atual + 1UL;
        }

        return atual > 0UL ? atual - 1UL : 0UL;
    }
}
