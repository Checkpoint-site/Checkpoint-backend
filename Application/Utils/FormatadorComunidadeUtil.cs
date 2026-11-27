using System.Globalization;
using System.Text;

namespace Application.Utils;

public static class FormatadorComunidadeUtil
{
    public static string FormatarTempoAtras(DateTime dataCriacaoUtc, DateTime? referenciaUtc = null)
    {
        var agora = referenciaUtc ?? DateTime.UtcNow;
        var utcCriacao = dataCriacaoUtc.Kind == DateTimeKind.Utc
            ? dataCriacaoUtc
            : DateTime.SpecifyKind(dataCriacaoUtc, DateTimeKind.Utc);

        var diferenca = agora - utcCriacao;
        if (diferenca.TotalMinutes < 1)
        {
            return "agora mesmo";
        }

        if (diferenca.TotalMinutes < 60)
        {
            return $"há {(int)Math.Floor(diferenca.TotalMinutes)} min";
        }

        if (diferenca.TotalHours < 24)
        {
            return $"há {(int)Math.Floor(diferenca.TotalHours)} h";
        }

        if (diferenca.TotalDays < 30)
        {
            return $"há {(int)Math.Floor(diferenca.TotalDays)} d";
        }

        return utcCriacao.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public static string ExtrairIniciais(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return "CP";
        }

        var partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 2)
        {
            return $"{char.ToUpperInvariant(partes[0][0])}{char.ToUpperInvariant(partes[1][0])}";
        }

        var limpo = partes[0];
        return limpo.Length >= 2
            ? limpo[..2].ToUpperInvariant()
            : limpo.ToUpperInvariant();
    }

    public static string GerarHandle(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return "@fogueira";
        }

        var normalizado = nome.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (categoria == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (ch is ' ' or '_' or '-')
            {
                if (sb.Length > 0 && sb[^1] != '_')
                {
                    sb.Append('_');
                }
            }
        }

        var slug = sb.ToString().Trim('_');
        if (string.IsNullOrWhiteSpace(slug))
        {
            return "@fogueira";
        }

        return $"@{slug[..Math.Min(18, slug.Length)]}";
    }
}
