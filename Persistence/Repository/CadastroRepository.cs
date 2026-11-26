using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Response;
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
            if (string.Equals(command.TipoPerfil, "Moderador", StringComparison.OrdinalIgnoreCase))
            {
                var moderador = new Moderador
                {
                    Nome = command.Nome,
                    Email = command.Email,
                    Senha = command.Senha,
                    ContaDeletada = false,
                    FotoPerfil = command.FotoPerfil,
                    CodigoCredencial = command.CodigoCredencial ?? string.Empty,
                    SetorModeracao = command.SetorModeracao ?? string.Empty,
                    TotalAcoesModeracao = 0
                };

                await context.Moderadores.AddAsync(moderador);
                await context.SaveChangesAsync();

                return new CadastroRespostDto
                {
                    Id = moderador.Id,
                    Nome = moderador.Nome,
                    Email = moderador.Email,
                    TipoPerfil = "Moderador"
                };
            }

            var usuarioRegular = new UsuarioRegular
            {
                Nome = command.Nome,
                Email = command.Email,
                Senha = command.Senha,
                ContaDeletada = false,
                FotoPerfil = command.FotoPerfil,
                LimiteListas = command.LimiteListasInicial
            };

            await context.UsuariosRegulares.AddAsync(usuarioRegular);
            await context.SaveChangesAsync();

            return new CadastroRespostDto
            {
                Id = usuarioRegular.Id,
                Nome = usuarioRegular.Nome,
                Email = usuarioRegular.Email,
                TipoPerfil = "Regular"
            };
        }
        catch (DbUpdateException)
        {
            return null;
        }
    }
}
