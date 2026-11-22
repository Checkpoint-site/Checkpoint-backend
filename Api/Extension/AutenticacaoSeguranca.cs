using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extension;

public static class AutenticacaoSeguranca
{
    public static IServiceCollection AddAutenticacaoSeguranca(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? Environment.GetEnvironmentVariable("Token__Secret")
            ?? (configuration["Token:Secret"] != "TOKEN_SECRET" ? configuration["Token:Secret"] : null)
            ?? Environment.GetEnvironmentVariable("TOKEN_SECRET");

        if (string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            throw new InvalidOperationException("A chave secreta do JWT não foi configurada.");
        }

        if (jwtSecretKey.Length < 32)
        {
            throw new InvalidOperationException("A chave secreta do JWT deve ter pelo menos 32 caracteres.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        string mensagemErro = context.AuthenticateFailure switch
                        {
                            SecurityTokenExpiredException => "O token expirou.",
                            SecurityTokenInvalidSignatureException => "Assinatura do token inválida.",
                            not null => "Falha na autenticação do token.",
                            null => "Token de autenticação não fornecido."
                        };

                        var payload = new
                        {
                            status = StatusCodes.Status401Unauthorized,
                            erro = "NaoAutorizado",
                            mensagem = mensagemErro
                        };

                        await context.Response.WriteAsJsonAsync(payload);
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
