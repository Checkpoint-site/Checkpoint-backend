using Domain.Dtos.Catalogo;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class CatalogoRepository(ApplicationDbContext context) : ICatalogoRepository
{
    public async Task<Jogo?> ObterPorIdAsync(int idJogo, CancellationToken cancellationToken = default)
    {
        return await context.Jogos
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdJogo == idJogo, cancellationToken);
    }

    public async Task<IReadOnlyList<Jogo>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default)
    {
        var listaIds = idsJogos.Distinct().ToList();
        if (listaIds.Count == 0)
        {
            return [];
        }

        return await context.Jogos
            .AsNoTracking()
            .Where(j => listaIds.Contains(j.IdJogo))
            .ToListAsync(cancellationToken);
    }

    public async Task<Jogo?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken cancellationToken = default)
    {
        return await context.Jogos
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.SteamAppId == steamAppId, cancellationToken);
    }

    public async Task<IReadOnlyList<Jogo>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken cancellationToken = default)
    {
        var listaIds = steamAppIds.Distinct().ToList();
        if (listaIds.Count == 0)
        {
            return [];
        }

        return await context.Jogos
            .AsNoTracking()
            .Where(j => listaIds.Contains(j.SteamAppId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Jogo>> BuscarCandidatosLocaisAsync(string tipoBusca, string termo, CancellationToken cancellationToken = default)
    {
        var filtro = termo.Trim();
        var tipoNormalizado = tipoBusca.Trim().ToLowerInvariant();

        IQueryable<Jogo> query = context.Jogos.AsNoTracking();

        query = tipoNormalizado switch
        {
            "ano" when int.TryParse(filtro, out var ano) => query.Where(j => j.AnoLancamento == ano),
            "ano" => query.Where(_ => false),
            "autor" => query.Where(j => j.Desenvolvedora.Contains(filtro) || j.Publicadora.Contains(filtro)),
            _ => query.Where(j => j.Nome.Contains(filtro))
        };

        return await query
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task<Jogo> SalvarOuAtualizarAsync(DetalhesJogoSteamDto detalhesSteam, CancellationToken cancellationToken = default)
    {
        var existente = await context.Jogos
            .FirstOrDefaultAsync(j => j.SteamAppId == detalhesSteam.SteamAppId, cancellationToken);

        if (existente is null)
        {
            var novoJogo = new Jogo
            {
                SteamAppId = detalhesSteam.SteamAppId,
                Nome = detalhesSteam.Nome,
                Descricao = detalhesSteam.DescricaoCurta,
                Imagem = detalhesSteam.ImagemCapaUrl,
                Desenvolvedora = detalhesSteam.Desenvolvedora,
                Publicadora = detalhesSteam.Publicadora,
                Generos = detalhesSteam.Generos.Distinct().ToList(),
                DataLancamentoTexto = detalhesSteam.DataLancamentoTexto,
                AnoLancamento = detalhesSteam.AnoLancamento,
                NotaMetacritic = detalhesSteam.NotaMetacritic,
                TotalRecomendacoes = detalhesSteam.TotalRecomendacoes,
                Gratuito = detalhesSteam.Gratuito,
                EmBreve = detalhesSteam.EmBreve,
                SincronizadoEm = DateTime.UtcNow
            };

            await context.Jogos.AddAsync(novoJogo, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var possuiLogger = await context.JogoLoggers
                .AnyAsync(l => l.IdJogo == novoJogo.IdJogo, cancellationToken);

            if (!possuiLogger)
            {
                await context.JogoLoggers.AddAsync(new JogoLogger
                {
                    IdJogo = novoJogo.IdJogo,
                    UltimaAtualizacao = DateTime.UtcNow
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }

            return novoJogo;
        }

        existente.Nome = detalhesSteam.Nome;
        existente.Descricao = detalhesSteam.DescricaoCurta;
        existente.Imagem = detalhesSteam.ImagemCapaUrl;
        existente.Desenvolvedora = detalhesSteam.Desenvolvedora;
        existente.Publicadora = detalhesSteam.Publicadora;
        existente.Generos = detalhesSteam.Generos.Distinct().ToList();
        existente.DataLancamentoTexto = detalhesSteam.DataLancamentoTexto;
        existente.AnoLancamento = detalhesSteam.AnoLancamento;
        existente.NotaMetacritic = detalhesSteam.NotaMetacritic;
        existente.TotalRecomendacoes = detalhesSteam.TotalRecomendacoes;
        existente.Gratuito = detalhesSteam.Gratuito;
        existente.EmBreve = detalhesSteam.EmBreve;
        existente.SincronizadoEm = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return existente;
    }
}
