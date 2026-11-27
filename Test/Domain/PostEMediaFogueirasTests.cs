using System.Reflection;
using Application.Utils;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Test.Domain;

public class PostEMediaFogueirasTests
{
    [Fact]
    public void TodosOsModelosDoDominio_PossuemApenasPropriedadesSemMetodosDeLogica()
    {
        var modelos = typeof(Post).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "Domain.Model" && t.IsClass)
            .ToList();

        Assert.NotEmpty(modelos);

        foreach (var modelo in modelos)
        {
            var metodosProprios = modelo
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => m.Name)
                .ToList();

            Assert.True(
                metodosProprios.Count == 0,
                $"O modelo '{modelo.Name}' não deve conter métodos de lógica, mas possui: {string.Join(", ", metodosProprios)}");
        }
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.5)]
    [InlineData(4.5)]
    [InlineData(5.0)]
    public void ValidarFogueiras_AceitaNotasComPrecisaoDeMeiaChama(double nota)
    {
        var ex = Record.Exception(() => AvaliacaoFogueiraUtil.ValidarFogueiras(nota));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.3)]
    [InlineData(3.7)]
    [InlineData(5.5)]
    [InlineData(-1.0)]
    public void ValidarFogueiras_RejeitaValoresForaDaEscalaOuSemPassoDeMeiaChama(double notaInvalida)
    {
        Assert.Throws<ValidationException>(() => AvaliacaoFogueiraUtil.ValidarFogueiras(notaInvalida));
    }

    [Fact]
    public void AvaliacaoFogueiraUtil_CalculaMediaFogueirasComPrecisaoDecimal()
    {
        var jogo = new Jogo
        {
            SteamAppId = 1245620,
            Nome = "Elden Ring",
            Generos = [Generos.Rpg, Generos.Acao],
            Descricao = "Action RPG",
            Imagem = "https://steam/elden.jpg",
            Desenvolvedora = "FromSoftware",
            Publicadora = "Bandai Namco"
        };

        AvaliacaoFogueiraUtil.AtualizarMediaFogueirasJogo(jogo, somaFogueiras: 13.5, totalAvaliacoes: 3);
        Assert.Equal(4.5, jogo.MediaFogueiras);
        Assert.Equal(3, jogo.TotalAvaliacoes);

        var logger = new JogoLogger
        {
            IdJogo = 1,
            SomaFogueiras = 8.5,
            TotalAvaliacoes = 2,
            PresencaPost = 1
        };
        AvaliacaoFogueiraUtil.RecalcularMediaFogueirasLogger(logger);

        Assert.Equal(4.25, logger.MediaFogueiras);
    }

    [Fact]
    public void UtilitariosDeApplication_SuportamAtualizacaoDePostTagsESpoilerSemLogicaNosModels()
    {
        var post = new Post
        {
            IdUsuario = 10,
            IdJogo = 20,
            Fogueiras = 4.0,
            Conteudo = "Review inicial",
            TipoPost = [Generos.Rpg],
            DataCriacao = DateTime.UtcNow
        };

        AvaliacaoFogueiraUtil.AtualizarConteudoPost(
            post,
            titulo: null,
            fogueiras: 4.5,
            conteudo: "O chefe final tem uma trilha sonora inesquecível!",
            statusJornada: StatusJornada.Zerado,
            tags: ["Trilha sonora", "Desafiador", "trilha sonora"],
            temSpoiler: true);

        Assert.Equal(4.5, post.Fogueiras);
        Assert.Equal(StatusJornada.Zerado, post.StatusJornada);
        Assert.Equal(2, post.Tags.Count);
        Assert.True(post.TemSpoiler);
        Assert.NotEmpty(post.Titulo);

        var comentario = new Comentario
        {
            IdPost = 1,
            IdUsuario = 11,
            Conteudo = "A segunda fase me pegou de surpresa!",
            TemSpoiler = true,
            DataCriacao = DateTime.UtcNow
        };

        Assert.True(comentario.TemSpoiler);
        RegrasModeracaoUtil.AlterarMarcaSpoilerComentario(comentario, false);
        Assert.False(comentario.TemSpoiler);
    }
}
