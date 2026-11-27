using Application.Dtos.Catalogo.Responses;
using Application.Dtos.Vitrine.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Strategies.Algoritmos;
using Application.Utils;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Application.Services;

public class VitrineService(
    ICatalogoRepository catalogoRepository,
    IPostRepository postRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService,
    IAlgoritmoCuradoriaFactory algoritmoCuradoriaFactory
) : IVitrineService
{
    private const int MinimoJogosLocaisParaVitrineCompleta = 10;

    private static readonly IReadOnlyDictionary<Generos, (string Slug, string Nome, string TituloSecao, string SubtituloSecao)> MetadadosGenero =
        new Dictionary<Generos, (string, string, string, string)>
        {
            [Generos.Rpg] = ("rpg", "RPG", "RPGs Inesquecíveis", "Mundos densos e escolhas que ficam na memória"),
            [Generos.Acao] = ("acao", "Ação", "Ação & Adrenalina", "Combates intensos e ritmo eletrizante"),
            [Generos.Aventura] = ("aventura", "Aventura", "Grandes Jornadas", "Exploração e histórias marcantes"),
            [Generos.Indie] = ("indie", "Indie", "Indies que Marcaram", "Joias criativas feitas com alma"),
            [Generos.Estrategia] = ("estrategia", "Estratégia", "Mentes Táticas", "Planejamento e decisões cirúrgicas"),
            [Generos.Simulacao] = ("simulacao", "Simulação", "Mundos Vivos & Simulação", "Construa, gerencie e experimente no seu ritmo"),
            [Generos.Corrida] = ("corrida", "Corrida", "Alta Velocidade", "Disputas acirradas em cada curva"),
            [Generos.Esportes] = ("esportes", "Esportes", "Arena Competitiva", "Habilidade e espírito esportivo"),
            [Generos.Casual] = ("casual", "Casual", "Para Relaxar na Fogueira", "Partidas leves e divertidas"),
            [Generos.MassivoMultiplayer] = ("coop", "MMO & Co-op", "Jornadas Compartilhadas", "Aventuras online ao lado da comunidade")
        };

    public async Task<VitrineHomeResponseDto> ObterVitrineHomeAsync(
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        var jogosLocais = (await catalogoRepository.ListarCatalogoLocalAsync(60, cancellationToken)).ToList();

        var reviewsRecentes = (await postRepository.ListarRecentesAsync(12, idUsuarioAtual, cancellationToken))
            .Select(PostMapper.ToPostResponse)
            .ToList();

        if (jogosLocais.Count == 0)
        {
            return new VitrineHomeResponseDto
            {
                Destaques = [],
                GenerosDisponiveis = [new GeneroVitrineResponseDto { Slug = "todos", Nome = "Todos" }],
                Secoes =
                [
                    new SecaoVitrineResponseDto
                    {
                        Id = "reviews-comunidade",
                        Titulo = "Na Fogueira Agora",
                        Subtitulo = "O que a galera acabou de zerar e avaliar",
                        Tipo = "REVIEWS",
                        Reviews = reviewsRecentes
                    }
                ]
            };
        }

        var idsJogos = jogosLocais.Select(j => j.IdJogo).Distinct().ToList();
        var jogoLoggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(idsJogos, cancellationToken);
        var generoLoggers = await loggerRepository.ObterGeneroLoggersAsync(cancellationToken);

        var ofertas = new Dictionary<int, OfertaJogoSteamDto>();
        foreach (var jogo in jogosLocais)
        {
            var oferta = await cacheService.ObterAsync<OfertaJogoSteamDto>(
                CatalogoService.ObterChaveOferta(jogo.SteamAppId),
                cancellationToken);
            if (oferta is not null)
            {
                ofertas[jogo.SteamAppId] = oferta;
            }
        }

        var contexto = new ContextoCuradoria
        {
            JogoLoggersPorJogo = jogoLoggers,
            GeneroLoggers = generoLoggers.ToDictionary(g => g.Genero),
            OfertasPorSteamAppId = ofertas
        };

        var algoritmoPopularidade = algoritmoCuradoriaFactory.Criar("popularidade");
        var candidatos = jogosLocais.Select(j => (Jogo: j, Afinidade: 1.0)).ToList();
        var ordenadosPrincipais = algoritmoPopularidade
            .Ordenar(candidatos, contexto)
            .Select(r =>
            {
                jogoLoggers.TryGetValue(r.Jogo.IdJogo, out var logger);
                var total = logger is { TotalAvaliacoes: > 0 } ? (int)logger.TotalAvaliacoes : r.Jogo.TotalAvaliacoes;
                return CatalogoMapper.ToResumoResponse(r.Jogo, r.PontuacaoCuradoria, r.MediaFogueiras, r.Oferta, total);
            })
            .ToList();

        if (ordenadosPrincipais.Count < MinimoJogosLocaisParaVitrineCompleta)
        {
            var complemento = await ObterComplementoFallbackMemoriaAsync(
                ordenadosPrincipais.Select(j => j.SteamAppId).ToHashSet(),
                algoritmoPopularidade,
                contexto.GeneroLoggers,
                cancellationToken);

            ordenadosPrincipais.AddRange(complemento);
        }

        return MontarVitrineCompleta(
            ordenadosPrincipais,
            reviewsRecentes,
            contexto,
            tituloTop10: "Top 10 na Fogueira Hoje",
            subtituloTop10: "Os jogos mais registrados e avaliados pela comunidade",
            embaralharDesempateGeneros: false);
    }

    public async Task<VitrineHomeResponseDto> ObterVitrineFallbackAsync(
        int limite = 24,
        string? algoritmo = null,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        if (limite <= 0 || limite > 60)
        {
            throw new ValidationException("O limite da vitrine de fallback deve estar entre 1 e 60 jogos.");
        }

        var algoritmoEscolhido = algoritmoCuradoriaFactory.Criar(algoritmo);
        var generoLoggers = await loggerRepository.ObterGeneroLoggersAsync(cancellationToken);
        var mapaGeneros = generoLoggers.ToDictionary(g => g.Genero);

        var poolSteam = await ProvedorFallbackSteam.ObterPoolAsync(
            catalogoRepository,
            steamStoreService,
            cacheService,
            cancellationToken);

        var ofertas = new Dictionary<int, OfertaJogoSteamDto>();
        var candidatos = new List<(JogoCatalogoRespostDto Jogo, double PesoRelevancia)>();

        foreach (var item in poolSteam)
        {
            candidatos.Add((item.Jogo, item.PesoRelevanciaFonte));
            if (item.Oferta is not null)
            {
                ofertas[item.Jogo.SteamAppId] = item.Oferta;
            }
        }

        if (candidatos.Count == 0)
        {
            var locaisResilientes = await catalogoRepository.ListarCatalogoLocalAsync(limite, cancellationToken);
            candidatos.AddRange(locaisResilientes.Select(j => (j, 1.0)));
        }

        var idsLocaisPresentes = candidatos
            .Where(c => c.Jogo.IdJogo > 0)
            .Select(c => c.Jogo.IdJogo)
            .Distinct()
            .ToList();

        var jogoLoggers = idsLocaisPresentes.Count > 0
            ? await loggerRepository.ObterJogoLoggersPorJogosAsync(idsLocaisPresentes, cancellationToken)
            : new Dictionary<int, Domain.Dtos.Loggers.Respost.JogoLoggerRespostDto>();

        var contexto = new ContextoCuradoria
        {
            JogoLoggersPorJogo = jogoLoggers,
            GeneroLoggers = mapaGeneros,
            OfertasPorSteamAppId = ofertas
        };

        var jogosOrdenados = ProvedorFallbackSteam
            .OrdenarComRelevanciaEAleatoriedade(candidatos, algoritmoEscolhido, contexto)
            .Take(limite)
            .ToList();

        var reviewsRecentes = (await postRepository.ListarRecentesAsync(12, idUsuarioAtual, cancellationToken))
            .Select(PostMapper.ToPostResponse)
            .ToList();

        return MontarVitrineCompleta(
            jogosOrdenados,
            reviewsRecentes,
            contexto,
            tituloTop10: "Descobertas em Alta",
            subtituloTop10: "Jogos relevantes selecionados dinamicamente na Steam enquanto o catálogo da Fogueira cresce",
            embaralharDesempateGeneros: true);
    }

    private async Task<IReadOnlyList<JogoResumoResponseDto>> ObterComplementoFallbackMemoriaAsync(
        HashSet<int> steamIdsJaIncluidos,
        AlgoritmoCuradoriaCatalogo algoritmo,
        IReadOnlyDictionary<Generos, Domain.Dtos.Loggers.Respost.GeneroLoggerRespostDto> generoLoggers,
        CancellationToken cancellationToken)
    {
        var pool = await ProvedorFallbackSteam.ObterPoolAsync(
            catalogoRepository,
            steamStoreService,
            cacheService,
            cancellationToken);

        var ineditos = pool
            .Where(p => !steamIdsJaIncluidos.Contains(p.Jogo.SteamAppId))
            .ToList();

        if (ineditos.Count == 0)
        {
            return [];
        }

        var ofertas = ineditos
            .Where(i => i.Oferta is not null)
            .ToDictionary(i => i.Jogo.SteamAppId, i => i.Oferta!);

        var contextoFallback = new ContextoCuradoria
        {
            GeneroLoggers = generoLoggers,
            OfertasPorSteamAppId = ofertas
        };

        return ProvedorFallbackSteam.OrdenarComRelevanciaEAleatoriedade(
            ineditos.Select(i => (i.Jogo, i.PesoRelevanciaFonte)),
            algoritmo,
            contextoFallback);
    }

    private static VitrineHomeResponseDto MontarVitrineCompleta(
        IReadOnlyList<JogoResumoResponseDto> jogosOrdenados,
        IReadOnlyList<Application.Dtos.Posts.Responses.PostResponseDto> reviewsRecentes,
        ContextoCuradoria contexto,
        string tituloTop10,
        string subtituloTop10,
        bool embaralharDesempateGeneros)
    {
        var destaquesComSinopse = jogosOrdenados
            .Where(j => !string.IsNullOrWhiteSpace(j.Descricao) && !string.Equals(j.Descricao, j.Nome, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        var destaques = destaquesComSinopse.Count >= 3
            ? destaquesComSinopse
            : jogosOrdenados.Take(Math.Min(5, jogosOrdenados.Count)).ToList();

        var top10 = jogosOrdenados.Take(10).ToList();
        var secoes = new List<SecaoVitrineResponseDto>();

        if (top10.Count > 0)
        {
            secoes.Add(new SecaoVitrineResponseDto
            {
                Id = "top-10-fogueira",
                Titulo = tituloTop10,
                Subtitulo = subtituloTop10,
                Tipo = "TOP_10",
                Jogos = top10
            });
        }

        secoes.Add(new SecaoVitrineResponseDto
        {
            Id = "reviews-comunidade",
            Titulo = "Na Fogueira Agora",
            Subtitulo = "O que a galera acabou de zerar e avaliar",
            Tipo = "REVIEWS",
            Reviews = reviewsRecentes
        });

        var generosOrdenados = MetadadosGenero.Keys
            .Select(g =>
            {
                contexto.GeneroLoggers.TryGetValue(g, out var gl);
                var desempate = embaralharDesempateGeneros ? Random.Shared.NextDouble() : 0.0;
                return new
                {
                    Genero = g,
                    Forca = ContextoCuradoria.CalcularForcaGeneroLogger(gl),
                    Desempate = desempate
                };
            })
            .OrderByDescending(x => x.Forca)
            .ThenByDescending(x => x.Desempate)
            .ThenBy(x => x.Genero)
            .Select(x => x.Genero)
            .ToList();

        var generosDisponiveis = new List<GeneroVitrineResponseDto>
        {
            new() { Slug = "todos", Nome = "Todos" }
        };

        foreach (var genero in generosOrdenados)
        {
            var meta = MetadadosGenero[genero];
            var nomeGeneroEnum = genero.ToString();
            var jogosDoGenero = jogosOrdenados
                .Where(j => j.Generos.Contains(nomeGeneroEnum, StringComparer.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            if (jogosDoGenero.Count > 0)
            {
                generosDisponiveis.Add(new GeneroVitrineResponseDto
                {
                    Slug = meta.Slug,
                    Nome = meta.Nome
                });

                if (secoes.Count(s => s.Tipo == "JOGOS") < 4)
                {
                    secoes.Add(new SecaoVitrineResponseDto
                    {
                        Id = $"genero-{meta.Slug}",
                        Titulo = meta.TituloSecao,
                        Subtitulo = meta.SubtituloSecao,
                        Tipo = "JOGOS",
                        FiltroGenero = meta.Slug,
                        Jogos = jogosDoGenero
                    });
                }
            }
        }

        return new VitrineHomeResponseDto
        {
            Destaques = destaques,
            GenerosDisponiveis = generosDisponiveis,
            Secoes = secoes
        };
    }
}
