namespace Api.Extension;

public static class ConexaoRedis
{
    public static IServiceCollection AddConexaoRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "Checkpoint:";
        });

        return services;
    }
}
