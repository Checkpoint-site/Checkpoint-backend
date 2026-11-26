using Domain.Enum;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class SolicitacaoModeradorRepository(ApplicationDbContext context) : ISolicitacaoModeradorRepository
{
    public async Task<Usuario?> ObterUsuarioPorIdAsync(int idUsuario)
    {
        return await context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == idUsuario);
    }

    public async Task<Post?> ObterPostPorIdAsync(int idPost)
    {
        return await context.Posts
            .FirstOrDefaultAsync(p => p.IdPost == idPost);
    }

    public async Task<bool> ExisteSolicitacaoAbertaAsync(int idUsuarioSolicitante)
    {
        return await context.SolicitacoesModerador
            .AsNoTracking()
            .AnyAsync(s => s.IdUsuarioSolicitante == idUsuarioSolicitante
                && (s.Status == StatusSolicitacaoModerador.Pendente
                    || s.Status == StatusSolicitacaoModerador.AguardandoSegundaAprovacao));
    }

    public async Task<SolicitacaoModerador?> ObterPorIdAsync(int idSolicitacao)
    {
        return await context.SolicitacoesModerador
            .Include(s => s.UsuarioSolicitante)
            .Include(s => s.PrimeiroAprovador)
            .Include(s => s.SegundoAprovador)
            .Include(s => s.AvaliadorRecusa)
            .FirstOrDefaultAsync(s => s.Id == idSolicitacao);
    }

    public async Task<SolicitacaoModerador?> ObterUltimaPorUsuarioAsync(int idUsuarioSolicitante)
    {
        return await context.SolicitacoesModerador
            .AsNoTracking()
            .Include(s => s.UsuarioSolicitante)
            .Include(s => s.PrimeiroAprovador)
            .Include(s => s.SegundoAprovador)
            .Where(s => s.IdUsuarioSolicitante == idUsuarioSolicitante)
            .OrderByDescending(s => s.DataCriacao)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<SolicitacaoModerador>> ListarAsync(StatusSolicitacaoModerador? status = null)
    {
        var query = context.SolicitacoesModerador
            .AsNoTracking()
            .Include(s => s.UsuarioSolicitante)
            .Include(s => s.PrimeiroAprovador)
            .Include(s => s.SegundoAprovador)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }
        else
        {
            query = query.Where(s =>
                s.Status == StatusSolicitacaoModerador.Pendente
                || s.Status == StatusSolicitacaoModerador.AguardandoSegundaAprovacao);
        }

        return await query
            .OrderBy(s => s.DataCriacao)
            .ToListAsync();
    }

    public async Task<SolicitacaoModerador> CriarAsync(SolicitacaoModerador solicitacao)
    {
        await context.SolicitacoesModerador.AddAsync(solicitacao);
        await context.SaveChangesAsync();
        return solicitacao;
    }

    public async Task SalvarAlteracoesAsync()
    {
        await context.SaveChangesAsync();
    }
}
