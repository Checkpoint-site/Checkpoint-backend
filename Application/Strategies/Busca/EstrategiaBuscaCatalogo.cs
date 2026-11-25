using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Strategies.Busca;

public abstract class EstrategiaBuscaCatalogo
{
    public abstract string TipoBusca { get; }

    public virtual void ValidarTermo(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            throw new ValidationException("O termo de busca é obrigatório.");
        }

        var normalizado = termo.Trim();
        if (normalizado.Length < 2 || normalizado.Length > 100)
        {
            throw new ValidationException("O termo de busca deve conter entre 2 e 100 caracteres.");
        }
    }

    public abstract bool AtendeCriterio(Jogo jogo, string termo);

    public abstract double CalcularAfinidade(Jogo jogo, string termo);

    public IReadOnlyList<(Jogo Jogo, double Afinidade)> FiltrarCandidatos(IEnumerable<Jogo> candidatos, string termo)
    {
        ValidarTermo(termo);

        var resultado = new List<(Jogo Jogo, double Afinidade)>();

        foreach (var jogo in candidatos)
        {
            if (AtendeCriterio(jogo, termo))
            {
                var afinidade = CalcularAfinidade(jogo, termo);
                resultado.Add((jogo, afinidade));
            }
        }

        return resultado;
    }
}
