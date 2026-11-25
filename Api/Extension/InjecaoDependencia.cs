using Application.Factories;
using Application.Interfaces;
using Application.Services;
using Application.Strategies.Algoritmos;
using Application.Strategies.Busca;
using Application.Strategies.Estantes;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Persistence.Repository;
using Persistence.Services;

namespace Api.Extension;

public static class InjecaoDependencia
{
    public static IServiceCollection AddInjecaoDependencia(this IServiceCollection services)
    {
        services.AddScoped<ILoginRepository, LoginRepository>();
        services.AddScoped<ILoginService, LoginService>();

        services.AddScoped<ICadastroRepository, CadastroRepository>();
        services.AddScoped<ICodigoVerificacaoService, RedisCodigoVerificacaoService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ICadastroService, CadastroService>();

        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IEstanteRepository, EstanteRepository>();
        services.AddScoped<IEngajamentoLoggerRepository, EngajamentoLoggerRepository>();
        services.AddScoped<ICatalogoCacheService, RedisCatalogoCacheService>();

        services.AddHttpClient<ISteamStoreService, SteamStoreService>(client =>
        {
            client.BaseAddress = new Uri("https://store.steampowered.com/api/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CheckpointBackend/1.0");
        });

        services.AddScoped<EstrategiaBuscaCatalogo, BuscaPorNomeStrategy>();
        services.AddScoped<EstrategiaBuscaCatalogo, BuscaPorAnoStrategy>();
        services.AddScoped<EstrategiaBuscaCatalogo, BuscaPorAutorStrategy>();
        services.AddScoped<IBuscaCatalogoStrategyFactory, BuscaCatalogoStrategyFactory>();

        services.AddScoped<AlgoritmoCuradoriaCatalogo, AlgoritmoPopularidadeJogo>();
        services.AddScoped<AlgoritmoCuradoriaCatalogo, AlgoritmoTendenciaGenero>();
        services.AddScoped<AlgoritmoCuradoriaCatalogo, AlgoritmoCustoBeneficio>();
        services.AddScoped<IAlgoritmoCuradoriaFactory, AlgoritmoCuradoriaFactory>();

        services.AddScoped<RegraEstanteCatalogo, RegraEstanteFavoritos>();
        services.AddScoped<RegraEstanteCatalogo, RegraEstantePretendeJogar>();
        services.AddScoped<IRegraEstanteFactory, RegraEstanteFactory>();

        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IEstanteService, EstanteService>();

        return services;
    }
}
