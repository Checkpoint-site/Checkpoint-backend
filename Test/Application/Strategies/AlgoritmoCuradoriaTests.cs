using Application.Strategies.Algoritmos;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Loggers.Respost;
using Domain.Enum;

namespace Test.Application.Strategies;

public class AlgoritmoCuradoriaTests
{
    [Fact]
    public void PolimorfismoAlgoritmos_JogoChamativoDeGeneroFrioVenceEmPopularidade_EnquantoGeneroQuenteVenceEmTendencia()
    {
        var jogoCorridaFenomeno = new JogoCatalogoRespostDto
        {
            IdJogo = 1,
            SteamAppId = 1551360,
            Nome = "Forza Horizon 5",
            Descricao = "Corrida arcade",
            Imagem = "https://steam/forza.jpg",
            Desenvolvedora = "Playground Games",
            Publicadora = "Xbox Game Studios",
            Generos = [Generos.Corrida],
            NotaMetacritic = 92,
            TotalRecomendacoes = 150000
        };

        var jogoRpgComum = new JogoCatalogoRespostDto
        {
            IdJogo = 2,
            SteamAppId = 999999,
            Nome = "RPG Genérico",
            Descricao = "RPG padrão",
            Imagem = "https://steam/rpg.jpg",
            Desenvolvedora = "Estúdio X",
            Publicadora = "Estúdio X",
            Generos = [Generos.Rpg],
            NotaMetacritic = 72,
            TotalRecomendacoes = 500
        };

        var contexto = new ContextoCuradoria
        {
            JogoLoggersPorJogo = new Dictionary<int, JogoLoggerRespostDto>
            {
                [1] = new JogoLoggerRespostDto
                {
                    IdJogo = 1,
                    PresencaFavoritos = 40,
                    PresencaPost = 20,
                    SomaFogueiras = 96,
                    PresencaPretendeJogar = 30,
                    QuantidadeCurtida = 80
                },
                [2] = new JogoLoggerRespostDto
                {
                    IdJogo = 2,
                    PresencaFavoritos = 0,
                    PresencaPost = 0
                }
            },
            GeneroLoggers = new Dictionary<Generos, GeneroLoggerRespostDto>
            {
                [Generos.Corrida] = new GeneroLoggerRespostDto
                {
                    Genero = Generos.Corrida,
                    PresencaFavoritos = 1
                },
                [Generos.Rpg] = new GeneroLoggerRespostDto
                {
                    Genero = Generos.Rpg,
                    PresencaFavoritos = 80,
                    PresencaPost = 60,
                    QuantidadeCurtida = 200
                }
            }
        };

        var candidatos = new List<(JogoCatalogoRespostDto Jogo, double Afinidade)>
        {
            (jogoCorridaFenomeno, 1.0),
            (jogoRpgComum, 1.0)
        };

        AlgoritmoCuradoriaCatalogo algoPopularidade = new AlgoritmoPopularidadeJogo();
        AlgoritmoCuradoriaCatalogo algoTendencia = new AlgoritmoTendenciaGenero();

        var rankingPopularidade = algoPopularidade.Ordenar(candidatos, contexto);
        var rankingTendencia = algoTendencia.Ordenar(candidatos, contexto);

        Assert.Equal("Forza Horizon 5", rankingPopularidade[0].Jogo.Nome);
        Assert.Equal(4.8, rankingPopularidade[0].MediaFogueiras);
        Assert.Equal("RPG Genérico", rankingTendencia[0].Jogo.Nome);
    }

    [Fact]
    public void AlgoritmoTendenciaGenero_PriorizaGeneroComMaisPresencaEmPostsECatalogos()
    {
        var jogoAcao = new JogoCatalogoRespostDto
        {
            IdJogo = 10,
            SteamAppId = 100,
            Nome = "Jogo Ação Pouco Engajado",
            Descricao = "Ação",
            Imagem = "img",
            Desenvolvedora = "Dev",
            Publicadora = "Pub",
            Generos = [Generos.Acao],
            NotaMetacritic = 80
        };

        var jogoSimulacao = new JogoCatalogoRespostDto
        {
            IdJogo = 11,
            SteamAppId = 101,
            Nome = "Jogo Simulação Em Alta",
            Descricao = "Simulação",
            Imagem = "img",
            Desenvolvedora = "Dev",
            Publicadora = "Pub",
            Generos = [Generos.Simulacao],
            NotaMetacritic = 80
        };

        var contexto = new ContextoCuradoria
        {
            GeneroLoggers = new Dictionary<Generos, GeneroLoggerRespostDto>
            {
                [Generos.Acao] = new GeneroLoggerRespostDto { Genero = Generos.Acao, PresencaFavoritos = 2, PresencaPost = 1 },
                [Generos.Simulacao] = new GeneroLoggerRespostDto { Genero = Generos.Simulacao, PresencaFavoritos = 25, PresencaPost = 15 }
            }
        };

        AlgoritmoCuradoriaCatalogo algoritmo = new AlgoritmoTendenciaGenero();
        var ranking = algoritmo.Ordenar([(jogoAcao, 1.0), (jogoSimulacao, 1.0)], contexto);

        Assert.Equal("Jogo Simulação Em Alta", ranking[0].Jogo.Nome);
    }

    [Fact]
    public void AlgoritmoCustoBeneficio_UsaOfertaTemporariaDoRedisEPriorizaPromocaoOuGratuito()
    {
        var jogoCaro = new JogoCatalogoRespostDto
        {
            IdJogo = 1,
            SteamAppId = 10,
            Nome = "AAA Preço Cheio",
            Descricao = "Desc",
            Imagem = "img",
            Desenvolvedora = "Dev",
            Publicadora = "Pub",
            Generos = [Generos.Acao],
            Gratuito = false,
            NotaMetacritic = 85
        };

        var jogoEmSuperOferta = new JogoCatalogoRespostDto
        {
            IdJogo = 2,
            SteamAppId = 20,
            Nome = "RPG em Promoção Relâmpago",
            Descricao = "Desc",
            Imagem = "img",
            Desenvolvedora = "Dev",
            Publicadora = "Pub",
            Generos = [Generos.Rpg],
            Gratuito = false,
            NotaMetacritic = 85
        };

        var contexto = new ContextoCuradoria
        {
            OfertasPorSteamAppId = new Dictionary<int, OfertaJogoSteamDto>
            {
                [10] = new OfertaJogoSteamDto
                {
                    SteamAppId = 10,
                    PrecoOriginalCentavos = 34990,
                    PrecoFinalCentavos = 34990,
                    PercentualDesconto = 0
                },
                [20] = new OfertaJogoSteamDto
                {
                    SteamAppId = 20,
                    PrecoOriginalCentavos = 19990,
                    PrecoFinalCentavos = 1999,
                    PercentualDesconto = 90
                }
            }
        };

        AlgoritmoCuradoriaCatalogo algoritmo = new AlgoritmoCustoBeneficio();
        var ranking = algoritmo.Ordenar([(jogoCaro, 1.0), (jogoEmSuperOferta, 1.0)], contexto);

        Assert.Equal("RPG em Promoção Relâmpago", ranking[0].Jogo.Nome);
        Assert.Equal(90, ranking[0].Oferta?.PercentualDesconto);
    }
}
