using Application.Strategies.Estantes;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Test.Application.Strategies;

public class RegraEstanteCatalogoTests
{
    [Fact]
    public void PolimorfismoEstantes_JogoEmBreveEhBloqueadoEmFavoritosMasPermitidoEmPretendeJogar()
    {
        var jogoEmBreve = new Jogo
        {
            IdJogo = 50,
            SteamAppId = 1030300,
            Nome = "Hollow Knight: Silksong",
            Descricao = "Em breve",
            Imagem = "img",
            Desenvolvedora = "Team Cherry",
            Publicadora = "Team Cherry",
            Generos = [Generos.Indie, Generos.Aventura],
            EmBreve = true
        };

        var favoritos = new Favoritos { IdUsuario = 1, Jogos = [] };
        var pretendeJogar = new PretendeJogar { IdUsuario = 1, Jogos = [] };

        RegraEstanteCatalogo regraFavoritos = new RegraEstanteFavoritos();
        RegraEstanteCatalogo regraPretendeJogar = new RegraEstantePretendeJogar();

        Assert.Throws<ValidationException>(() => regraFavoritos.ValidarAdicaoJogo(favoritos, jogoEmBreve));
        var excecao = Record.Exception(() => regraPretendeJogar.ValidarAdicaoJogo(pretendeJogar, jogoEmBreve));
        Assert.Null(excecao);
    }

    [Fact]
    public void RegraEstanteFavoritos_AtualizarLoggers_IncrementaEDecrementaOsTresLoggersComSeguranca()
    {
        RegraEstanteCatalogo regra = new RegraEstanteFavoritos();
        var jogoLogger = new JogoLogger { IdJogo = 10 };
        var generoGlobal = new GeneroLogger { Genero = Generos.Rpg };
        var generoUsuario = new GeneroLogger { IdUsuario = 5, Genero = Generos.Rpg };
        var usuarioLogger = new UsuarioLogger { IdUsuario = 5 };

        regra.AtualizarLoggers(jogoLogger, [generoGlobal, generoUsuario], usuarioLogger, adicionando: true);

        Assert.Equal(1UL, jogoLogger.PresencaFavoritos);
        Assert.Equal(1UL, generoGlobal.PresencaFavoritos);
        Assert.Equal(1UL, generoUsuario.PresencaFavoritos);
        Assert.Equal(1UL, usuarioLogger.PresencaFavoritos);

        regra.AtualizarLoggers(jogoLogger, [generoGlobal, generoUsuario], usuarioLogger, adicionando: false);
        regra.AtualizarLoggers(jogoLogger, [generoGlobal, generoUsuario], usuarioLogger, adicionando: false);

        Assert.Equal(0UL, jogoLogger.PresencaFavoritos);
        Assert.Equal(0UL, generoGlobal.PresencaFavoritos);
        Assert.Equal(0UL, generoUsuario.PresencaFavoritos);
        Assert.Equal(0UL, usuarioLogger.PresencaFavoritos);
    }
}
