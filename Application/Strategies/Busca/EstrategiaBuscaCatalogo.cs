using System.Globalization;
using System.Text;
using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Busca;

public abstract class EstrategiaBuscaCatalogo
{
    public abstract TipoBuscaCatalogo TipoBusca { get; }

    public virtual string ChaveExibicao => TipoBusca.ToString().ToLowerInvariant();

    public static string NormalizarTextoBusca(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);

        foreach (var c in decomposto)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (c is '\'' or '’' or '`' or '®' or '™' or '©')
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
            else if (sb.Length > 0 && sb[^1] != ' ')
            {
                sb.Append(' ');
            }
        }

        return sb.ToString().Trim();
    }

    public static bool ContemTodosOsTermos(string textoNormalizado, string buscaNormalizada)
    {
        if (string.IsNullOrEmpty(textoNormalizado) || string.IsNullOrEmpty(buscaNormalizada))
        {
            return false;
        }

        if (textoNormalizado.Contains(buscaNormalizada, StringComparison.Ordinal))
        {
            return true;
        }

        var termos = buscaNormalizada.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return termos.Length > 1 && termos.All(t => textoNormalizado.Contains(t, StringComparison.Ordinal));
    }

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
