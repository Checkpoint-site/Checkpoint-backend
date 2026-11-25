using Application.Strategies.Busca;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Test.Application.Strategies;

public class BuscaCatalogoStrategyTests
{
    private static Jogo CriarJogo(int id, string nome, int ano, string dev, string pub) => new()
    {
        IdJogo = id,
        SteamAppId = id * 10,
        Nome = nome,
        Descricao = "Descrição",
        Imagem = "https://steam/header.jpg",
        Desenvolvedora = dev,
        Publicadora = pub,
        AnoLancamento = ano,
        Generos = [Generos.Rpg, Generos.Acao]
    };

    [Fact]
    public void PolimorfismoBusca_NomeAnoEAutor_FiltramEPontuamDeAcordoComCadaSubclasse()
    {
        var catalogo = new List<Jogo>
        {
            CriarJogo(1, "Elden Ring", 2022, "FromSoftware", "Bandai Namco"),
            CriarJogo(2, "Dark Souls III", 2016, "FromSoftware", "FromSoftware"),
            CriarJogo(3, "Tekken 8", 2024, "Bandai Namco Studios", "FromSoftware Publishing"),
            CriarJogo(4, "The Witcher 3: Wild Hunt", 2015, "CD PROJEKT RED", "CD PROJEKT RED")
        };

        EstrategiaBuscaCatalogo buscaNome = new BuscaPorNomeStrategy();
        EstrategiaBuscaCatalogo buscaAno = new BuscaPorAnoStrategy();
        EstrategiaBuscaCatalogo buscaAutor = new BuscaPorAutorStrategy();

        var porNome = buscaNome.FiltrarCandidatos(catalogo, "Elden Ring");
        Assert.Single(porNome);
        Assert.Equal(1.0, porNome[0].Afinidade);

        var porAno = buscaAno.FiltrarCandidatos(catalogo, "2015");
        Assert.Single(porAno);
        Assert.Equal("The Witcher 3: Wild Hunt", porAno[0].Jogo.Nome);

        var porAutor = buscaAutor.FiltrarCandidatos(catalogo, "FromSoftware");
        Assert.Equal(3, porAutor.Count);

        var afinidadeDevExata = porAutor.Single(x => x.Jogo.Nome == "Elden Ring").Afinidade;
        var afinidadeApenasPublicadora = porAutor.Single(x => x.Jogo.Nome == "Tekken 8").Afinidade;
        Assert.True(afinidadeDevExata > afinidadeApenasPublicadora);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    public void BuscaPorNome_TermoInvalido_LancaValidationException(string termo)
    {
        EstrategiaBuscaCatalogo estrategia = new BuscaPorNomeStrategy();
        Assert.Throws<ValidationException>(() => estrategia.ValidarTermo(termo));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1950")]
    [InlineData("9999")]
    public void BuscaPorAno_AnoInvalido_LancaValidationException(string ano)
    {
        EstrategiaBuscaCatalogo estrategia = new BuscaPorAnoStrategy();
        Assert.Throws<ValidationException>(() => estrategia.ValidarTermo(ano));
    }
}
