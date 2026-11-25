using Application.Factories;
using Application.Strategies.Algoritmos;
using Application.Strategies.Busca;
using Application.Strategies.Estantes;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Test.Application.Factories;

public class StrategyFactoriesTests
{
    [Fact]
    public void BuscaCatalogoStrategyFactory_ResolvePorEnumEPorStringERetornaPadraoNome()
    {
        var factory = new BuscaCatalogoStrategyFactory(
        [
            new BuscaPorNomeStrategy(),
            new BuscaPorAnoStrategy(),
            new BuscaPorAutorStrategy()
        ]);

        Assert.IsType<BuscaPorNomeStrategy>(factory.Criar((string?)null));
        Assert.IsType<BuscaPorAnoStrategy>(factory.Criar("ano"));
        Assert.IsType<BuscaPorAutorStrategy>(factory.Criar(TipoBuscaCatalogo.Autor));
        Assert.Throws<ValidationException>(() => factory.Criar("invalido"));
    }

    [Fact]
    public void AlgoritmoCuradoriaFactory_ResolvePorEnumEComHifenERetornaPadraoPopularidade()
    {
        var factory = new AlgoritmoCuradoriaFactory(
        [
            new AlgoritmoPopularidadeJogo(),
            new AlgoritmoTendenciaGenero(),
            new AlgoritmoCustoBeneficio()
        ]);

        Assert.IsType<AlgoritmoPopularidadeJogo>(factory.Criar((string?)null));
        Assert.IsType<AlgoritmoTendenciaGenero>(factory.Criar(TipoAlgoritmoCuradoria.Tendencia));
        Assert.IsType<AlgoritmoCustoBeneficio>(factory.Criar("custo-beneficio"));
        Assert.IsType<AlgoritmoCustoBeneficio>(factory.Criar("CustoBeneficio"));
        Assert.Throws<ValidationException>(() => factory.Criar("aleatorio"));
    }

    [Fact]
    public void RegraEstanteFactory_ResolvePorEnumEComHifenEBloqueiaEstanteInvalida()
    {
        var factory = new RegraEstanteFactory(
        [
            new RegraEstanteFavoritos(),
            new RegraEstantePretendeJogar()
        ]);

        Assert.IsType<RegraEstanteFavoritos>(factory.Criar(TipoEstante.Favoritos));
        Assert.IsType<RegraEstanteFavoritos>(factory.Criar("favoritos"));
        Assert.IsType<RegraEstantePretendeJogar>(factory.Criar("pretende-jogar"));
        Assert.IsType<RegraEstantePretendeJogar>(factory.Criar("PretendeJogar"));
        Assert.Throws<ValidationException>(() => factory.Criar(""));
        Assert.Throws<ValidationException>(() => factory.Criar("abandonados"));
    }
}
