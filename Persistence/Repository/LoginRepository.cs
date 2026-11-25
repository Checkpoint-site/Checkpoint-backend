using Domain.Dtos.Login.Commands;
using Domain.Dtos.Login.Response;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class LoginRepository(ApplicationDbContext context) : ILoginRepository
{
    public async Task<LoginRespostDto?> LoginRepositoryAsync(LoginCommandsDto command)
    {
        return await context.Usuarios
            .AsNoTracking()
            .Where(u => u.Email == command.Email && !u.ContaDeletada)
            .Select(u => new LoginRespostDto
            {
                Id = u.Id,
                Email = u.Email,
                Senha = u.Senha,
                TipoPerfil = u is Moderador ? "Moderador" : "Regular",
                SuspensoAte = u.SuspensoAte
            })
            .FirstOrDefaultAsync();
    }
}
