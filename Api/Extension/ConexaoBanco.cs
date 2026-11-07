using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Api.Extension;

public static class ConexaoBanco
{
    public static IServiceCollection AddConexaoBanco(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}

