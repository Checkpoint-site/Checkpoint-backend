using Api.Services;
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
        services.AddHostedService<AdminBootstrapHostedService>();

        services.AddScoped<ILoginRepository, LoginRepository>();
        services.AddScoped<ILoginService, LoginService>();

        services.AddScoped<ICadastroRepository, CadastroRepository>();
        services.AddScoped<ICodigoVerificacaoService, RedisCodigoVerificacaoService>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddHttpClient<IModeracaoFotoService, OpenAiModeracaoFotoService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddScoped<ICadastroService, CadastroService>();

        services.AddScoped<ISolicitacaoModeradorRepository, SolicitacaoModeradorRepository>();
        services.AddScoped<ISolicitacaoModeradorService, SolicitacaoModeradorService>();

        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IEstanteRepository, EstanteRepository>();
        services.AddScoped<IEngajamentoLoggerRepository, EngajamentoLoggerRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IDiarioRepository, DiarioRepository>();
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
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IDiarioService, DiarioService>();
        services.AddScoped<IVitrineService, VitrineService>();

        return services;
    }
}
