using Domain.Dtos.Login.Commands;
using Domain.Dtos.Login.Respost;
using Domain.Interfaces.Repository;
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
                Nome = u.Nome,
                Email = u.Email,
                Senha = u.Senha,
                TipoPerfil = u.TipoPerfil,
                SuspensoAte = u.SuspensoAte,
                FotoPerfil = u.FotoPerfil
            })
            .FirstOrDefaultAsync();
    }
}
