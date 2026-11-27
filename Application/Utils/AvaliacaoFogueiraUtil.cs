using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Utils;

public static class AvaliacaoFogueiraUtil
{
    public static void ValidarFogueiras(double fogueiras)
    {
        if (double.IsNaN(fogueiras) || double.IsInfinity(fogueiras) || fogueiras < 0.5 || fogueiras > 5.0)
        {
            throw new ValidationException("A nota de fogueiras deve estar entre 0.5 e 5.0.");
        }

        var dobro = fogueiras * 2.0;
        if (Math.Abs(dobro - Math.Round(dobro)) > 1e-6)
        {
            throw new ValidationException("A nota de fogueiras deve respeitar incrementos de meia chama (0.5).");
        }
    }

    public static double ArredondarMeiaChama(double fogueiras)
    {
        ValidarFogueiras(fogueiras);
        return Math.Round(fogueiras * 2.0) / 2.0;
    }

    public static List<string> NormalizarTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return [];
        }

        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Where(t => t.Length <= 40)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();
    }

    public static string ValidarENormalizarConteudoReview(string? conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new ValidationException("O conteúdo da review não pode ser vazio.");
        }

        var conteudoLimpo = conteudo.Trim();
        if (conteudoLimpo.Length > 4000)
        {
            throw new ValidationException("O conteúdo da review deve ter no máximo 4000 caracteres.");
        }

        return conteudoLimpo;
    }

    public static string GerarTituloAutomatico(string conteudo, string? nomeJogo = null)
    {
        var texto = conteudo.Trim();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var primeiraLinha = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (primeiraLinha.Length <= 80)
            {
                return primeiraLinha;
            }

            return $"{primeiraLinha[..77].TrimEnd()}...";
        }

        return string.IsNullOrWhiteSpace(nomeJogo)
            ? "Relato na Fogueira"
            : $"Relato sobre {nomeJogo.Trim()}";
    }

    public static string ResolverTituloReview(string? titulo, string conteudoLimpo, string? nomeJogo = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            return GerarTituloAutomatico(conteudoLimpo, nomeJogo);
        }

        var limpo = titulo.Trim();
        return limpo[..Math.Min(180, limpo.Length)];
    }

    public static void AtualizarConteudoPost(
        Post post,
        string? titulo,
        double fogueiras,
        string conteudo,
        StatusJornada? statusJornada,
        IEnumerable<string>? tags,
        bool temSpoiler,
        string? nomeJogo = null)
    {
        if (post.PostDeletado)
        {
            throw new ValidationException("Não é possível editar um post excluído.");
        }

        var notaArredondada = ArredondarMeiaChama(fogueiras);
        var conteudoLimpo = ValidarENormalizarConteudoReview(conteudo);

        post.Titulo = ResolverTituloReview(titulo, conteudoLimpo, nomeJogo);
        post.Fogueiras = notaArredondada;
        post.Conteudo = conteudoLimpo;
        post.StatusJornada = statusJornada;
        post.Tags = NormalizarTags(tags);
        post.TemSpoiler = temSpoiler;
        post.DataAtualizacao = DateTime.UtcNow;
    }

    public static void AtualizarMediaFogueirasJogo(Jogo jogo, double somaFogueiras, int totalAvaliacoes)
    {
        jogo.TotalAvaliacoes = Math.Max(0, totalAvaliacoes);
        jogo.MediaFogueiras = jogo.TotalAvaliacoes > 0
            ? Math.Round(Math.Clamp(somaFogueiras / jogo.TotalAvaliacoes, 0.0, 5.0), 2)
            : 0.0;
    }

    public static void RecalcularMediaFogueirasLogger(JogoLogger logger)
    {
        var divisor = logger.TotalAvaliacoes > 0 ? logger.TotalAvaliacoes : logger.PresencaPost;
        logger.MediaFogueiras = divisor > 0
            ? Math.Round(Math.Clamp(logger.SomaFogueiras / divisor, 0.0, 5.0), 2)
            : 0.0;
    }

    public static void RecalcularMediaFogueirasUsuarioLogger(UsuarioLogger logger)
    {
        var divisor = logger.TotalAvaliacoes > 0 ? logger.TotalAvaliacoes : logger.PresencaPost;
        logger.MediaFogueiras = divisor > 0
            ? Math.Round(Math.Clamp(logger.SomaFogueirasDadas / divisor, 0.0, 5.0), 2)
            : 0.0;
    }
}
