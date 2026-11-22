using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Response;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class CadastroRepository(ApplicationDbContext context) : ICadastroRepository
{
    public async Task<CadastroRespostDto?> CadastroRepositoryAsync(CadastroCommandsDto command)
    {
        var emailExiste = await context.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Email == command.Email);

        if (emailExiste)
        {
            return null;
        }

        var usuario = new Usuario
        {
            Nome = command.Nome,
            Email = command.Email,
            Senha = command.Senha,
            ContaDeletada = false
        };

        try
        {
            await context.Usuarios.AddAsync(usuario);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return new CadastroRespostDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        };
    }
}
