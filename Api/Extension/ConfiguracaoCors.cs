namespace Api.Extension;

public static class ConfiguracaoCors
{
    public const string NomePoliticaPadrao = "CheckpointFrontendPolicy";

    public static IServiceCollection AddConfiguracaoCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origensConfig = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var origensEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var origensPermitidas = (origensEnv is { Length: > 0 } ? origensEnv : origensConfig)
            ?? [
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            ];

        services.AddCors(options =>
        {
            options.AddPolicy(NomePoliticaPadrao, policy =>
            {
                policy.WithOrigins(origensPermitidas)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
