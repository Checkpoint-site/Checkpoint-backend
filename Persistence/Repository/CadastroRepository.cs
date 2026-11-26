using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
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
            if (command.TipoPerfil == TipoPerfilUsuario.Moderador)
            {
                var credencialNormalizada = command.CodigoCredencial?.Trim() ?? string.Empty;
                var credencialExiste = await context.Moderadores
                    .AsNoTracking()
                    .AnyAsync(m => m.CodigoCredencial == credencialNormalizada);

                if (credencialExiste)
                {
                    throw new ConflictException("Este código de credencial de moderador já está em uso.");
                }

                var moderador = new Moderador
                {
                    Nome = command.Nome,
                    Email = command.Email,
                    Senha = command.Senha,
                    ContaDeletada = false,
                    FotoPerfil = command.FotoPerfil,
                    CodigoCredencial = credencialNormalizada,
                    SetorModeracao = command.SetorModeracao?.Trim() ?? string.Empty,
                    TotalAcoesModeracao = 0
                };

                await context.Moderadores.AddAsync(moderador);
                await context.SaveChangesAsync();

                return new CadastroRespostDto
                {
                    Id = moderador.Id,
                    Nome = moderador.Nome,
                    Email = moderador.Email,
                    TipoPerfil = TipoPerfilUsuario.Moderador
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
                TipoPerfil = TipoPerfilUsuario.Regular
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
