using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Busca;

public abstract class EstrategiaBuscaCatalogo
{
    public abstract TipoBuscaCatalogo TipoBusca { get; }

    public virtual string ChaveExibicao => TipoBusca.ToString().ToLowerInvariant();

    public virtual void ValidarBusca(string jogoBuscado)
    {
        if (string.IsNullOrWhiteSpace(jogoBuscado))
        {
            throw new ValidationException("O jogo buscado é obrigatório.");
        }

        var normalizado = jogoBuscado.Trim();
        if (normalizado.Length < 2 || normalizado.Length > 100)
        {
            throw new ValidationException("O jogo buscado deve conter entre 2 e 100 caracteres.");
        }
    }

    public abstract bool AtendeCriterio(JogoCatalogoRespostDto jogo, string jogoBuscado);

    public abstract double CalcularAfinidade(JogoCatalogoRespostDto jogo, string jogoBuscado);

    public IReadOnlyList<(JogoCatalogoRespostDto Jogo, double Afinidade)> FiltrarCandidatos(IEnumerable<JogoCatalogoRespostDto> candidatos, string jogoBuscado)
    {
        ValidarBusca(jogoBuscado);

        var resultado = new List<(JogoCatalogoRespostDto Jogo, double Afinidade)>();

        foreach (var jogo in candidatos)
        {
            if (AtendeCriterio(jogo, jogoBuscado))
            {
                var afinidade = CalcularAfinidade(jogo, jogoBuscado);
                resultado.Add((jogo, afinidade));
            }
        }

        return resultado;
    }
}
