using Application.Interfaces;
using Application.Services;
using Domain.Interfaces.Repository;
using Persistence.Repository;

namespace Api.Extension;

public static class InjecaoDependencia
{
    public static IServiceCollection AddInjecaoDependencia(this IServiceCollection services)
    {
        services.AddScoped<ILoginRepository, LoginRepository>();
        services.AddScoped<ILoginService, LoginService>();

        return services;
    }
}
