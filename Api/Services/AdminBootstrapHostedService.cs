using Application.Utils.Hash;
using Domain.Enum;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Api.Services;

/// <summary>
/// Garante no startup da aplicação a existência do usuário Admin inicial
/// configurado via variáveis de ambiente (EMAIL_ADMIN, SENHA_ADMIN, NOME_ADMIN).
/// </summary>
public class AdminBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<AdminBootstrapHostedService> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var email = Environment.GetEnvironmentVariable("EMAIL_ADMIN")?.Trim();
        var senha = Environment.GetEnvironmentVariable("SENHA_ADMIN");
        var nome = Environment.GetEnvironmentVariable("NOME_ADMIN")?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            logger.LogInformation("Bootstrap de Admin ignorado: variáveis EMAIL_ADMIN/SENHA_ADMIN não configuradas.");
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var existente = await context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (existente is null)
            {
                var admin = new Usuario
                {
                    Nome = string.IsNullOrWhiteSpace(nome) ? "Administrador Checkpoint" : nome,
                    Email = email,
                    Senha = Hash.GerarHashSenha(senha),
                    TipoPerfil = TipoPerfilUsuario.Admin,
                    ContaDeletada = false
                };

                await context.Usuarios.AddAsync(admin, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Usuário Admin inicial provisionado com sucesso via bootstrap.");
                return;
            }

            if (existente.TipoPerfil != TipoPerfilUsuario.Admin || existente.ContaDeletada)
            {
                existente.TipoPerfil = TipoPerfilUsuario.Admin;
                existente.ContaDeletada = false;
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Usuário Admin inicial sincronizado com perfil Admin via bootstrap.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível executar o bootstrap do Admin inicial no startup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
