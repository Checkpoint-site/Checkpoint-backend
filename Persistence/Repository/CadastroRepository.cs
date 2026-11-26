using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Respost;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class CadastroRepository(ApplicationDbContext context) : ICadastroRepository
{
    public async Task<bool> EmailExisteAsync(string email)
    {
        return await context.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Email == email);
    }

    public async Task<CadastroRespostDto?> CadastroRepositoryAsync(CadastroCommandsDto command)
    {
        var emailExiste = await EmailExisteAsync(command.Email);
        if (emailExiste)
        {
            return null;
        }

        try
        {
            var usuario = new Usuario
            {
                Nome = command.Nome,
                Email = command.Email,
                Senha = command.Senha,
                TipoPerfil = command.TipoPerfil,
                ContaDeletada = false,
                FotoPerfil = command.FotoPerfil
            };

            await context.Usuarios.AddAsync(usuario);
            await context.SaveChangesAsync();

            return new CadastroRespostDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TipoPerfil = usuario.TipoPerfil
            };
        }
        catch (DbUpdateException)
        {
            if (await EmailExisteAsync(command.Email))
            {
                return null;
            }

            throw;
        }
    }
}
