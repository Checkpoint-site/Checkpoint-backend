using Application.Strategies.Estantes;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Estantes.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Test.Application.Strategies;

public class RegraEstanteCatalogoTests
{
    [Fact]
    public void PolimorfismoEstantes_JogoEmBreveEhBloqueadoEmFavoritosMasPermitidoEmPretendeJogar()
    {
        var jogoEmBreve = new JogoCatalogoRespostDto
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

        EstanteCatalogoRespostDto favoritos = new FavoritosRespostDto { IdUsuario = 1, Jogos = [] };
        EstanteCatalogoRespostDto pretendeJogar = new PretendeJogarRespostDto { IdUsuario = 1, Jogos = [] };

        RegraEstanteCatalogo regraFavoritos = new RegraEstanteFavoritos();
        RegraEstanteCatalogo regraPretendeJogar = new RegraEstantePretendeJogar();

        Assert.Throws<ValidationException>(() => regraFavoritos.ValidarAdicaoJogo(favoritos, jogoEmBreve));
        var excecao = Record.Exception(() => regraPretendeJogar.ValidarAdicaoJogo(pretendeJogar, jogoEmBreve));
        Assert.Null(excecao);
    }

    [Fact]
    public void RegraEstanteFavoritosEPretendeJogar_CriarComandoImpactoLoggers_DefineDeltasPolimorficosCorretamente()
    {
        var jogo = new JogoCatalogoRespostDto
        {
            IdJogo = 10,
            SteamAppId = 292030,
            Nome = "The Witcher 3",
            Descricao = "RPG",
            Imagem = "img",
            Desenvolvedora = "CDPR",
            Publicadora = "CDPR",
            Generos = [Generos.Rpg, Generos.Rpg, Generos.Aventura]
        };

        RegraEstanteCatalogo regraFavoritos = new RegraEstanteFavoritos();
        RegraEstanteCatalogo regraPretende = new RegraEstantePretendeJogar();

        var impactoAddFav = regraFavoritos.CriarComandoImpactoLoggers(idUsuario: 5, jogo, adicionando: true);
        Assert.Equal(1, impactoAddFav.DeltaFavoritosJogo);
        Assert.Equal(1, impactoAddFav.DeltaFavoritosGenero);
        Assert.Equal(1, impactoAddFav.DeltaFavoritosUsuario);
        Assert.Equal(0, impactoAddFav.DeltaPretendeJogarJogo);
        Assert.Equal(2, impactoAddFav.Generos.Count);

        var impactoRemFav = regraFavoritos.CriarComandoImpactoLoggers(idUsuario: 5, jogo, adicionando: false);
        Assert.Equal(-1, impactoRemFav.DeltaFavoritosJogo);
        Assert.Equal(-1, impactoRemFav.DeltaFavoritosGenero);
        Assert.Equal(-1, impactoRemFav.DeltaFavoritosUsuario);

        var impactoAddPretende = regraPretende.CriarComandoImpactoLoggers(idUsuario: 5, jogo, adicionando: true);
        Assert.Equal(1, impactoAddPretende.DeltaPretendeJogarJogo);
        Assert.Equal(1, impactoAddPretende.DeltaPretendeJogarGenero);
        Assert.Equal(0, impactoAddPretende.DeltaFavoritosUsuario);
    }
}
