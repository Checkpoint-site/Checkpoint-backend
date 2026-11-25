using Application.Interfaces;
using Application.Services;
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

        services.AddHttpClient<IModeracaoFotoService, OpenAiModeracaoFotoService>();

        services.AddScoped<ICadastroService, CadastroService>();

        return services;
    }
}
