using System.Linq.Expressions;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Commands;
using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class CatalogoRepository(ApplicationDbContext context) : ICatalogoRepository
{
    private static readonly Expression<Func<Jogo, JogoCatalogoRespostDto>> ProjecaoJogoRespost = j => new JogoCatalogoRespostDto
    {
        IdJogo = j.IdJogo,
        SteamAppId = j.SteamAppId,
        Nome = j.Nome,
        Generos = j.Generos,
        Descricao = j.Descricao,
        Imagem = j.Imagem,
        Desenvolvedora = j.Desenvolvedora,
        Publicadora = j.Publicadora,
        DataLancamentoTexto = j.DataLancamentoTexto,
        AnoLancamento = j.AnoLancamento,
        NotaMetacritic = j.NotaMetacritic,
        TotalRecomendacoes = j.TotalRecomendacoes,
        Gratuito = j.Gratuito,
        EmBreve = j.EmBreve,
        MediaFogueiras = j.MediaFogueiras,
        TotalAvaliacoes = j.TotalAvaliacoes,
        SincronizadoEm = j.SincronizadoEm
    };

    public async Task<JogoCatalogoRespostDto?> ObterPorIdAsync(int idJogo, CancellationToken cancellationToken = default)
    {
        return await context.Jogos
            .AsNoTracking()
            .Where(j => j.IdJogo == idJogo)
            .Select(ProjecaoJogoRespost)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default)
    {
        var listaIds = idsJogos.Distinct().ToList();
        if (listaIds.Count == 0)
        {
            return [];
        }

        return await context.Jogos
            .AsNoTracking()
            .Where(j => listaIds.Contains(j.IdJogo))
            .Select(ProjecaoJogoRespost)
            .ToListAsync(cancellationToken);
    }

    public async Task<JogoCatalogoRespostDto?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken cancellationToken = default)
    {
        return await context.Jogos
            .AsNoTracking()
            .Where(j => j.SteamAppId == steamAppId)
            .Select(ProjecaoJogoRespost)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken cancellationToken = default)
    {
        var listaIds = steamAppIds.Distinct().ToList();
        if (listaIds.Count == 0)
        {
            return [];
        }

        return await context.Jogos
            .AsNoTracking()
            .Where(j => listaIds.Contains(j.SteamAppId))
            .Select(ProjecaoJogoRespost)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JogoCatalogoRespostDto>> BuscarCandidatosLocaisAsync(BuscarCandidatosLocaisCommandDto command, CancellationToken cancellationToken = default)
    {
        var filtro = command.JogoBuscado.Trim();

        IQueryable<Jogo> query = context.Jogos.AsNoTracking();

        query = command.TipoBusca switch
        {
            TipoBuscaCatalogo.Ano when int.TryParse(filtro, out var ano) => query.Where(j => j.AnoLancamento == ano),
            TipoBuscaCatalogo.Ano => query.Where(_ => false),
            TipoBuscaCatalogo.Autor => query.Where(j => j.Desenvolvedora.Contains(filtro) || j.Publicadora.Contains(filtro)),
            _ => query.Where(j => j.Nome.Contains(filtro))
        };

        return await query
            .Take(50)
            .Select(ProjecaoJogoRespost)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JogoCatalogoRespostDto>> ListarCatalogoLocalAsync(int limite = 60, CancellationToken cancellationToken = default)
    {
        var quantidade = Math.Clamp(limite, 1, 200);

        return await context.Jogos
            .AsNoTracking()
            .OrderByDescending(j => j.TotalAvaliacoes)
            .ThenByDescending(j => j.MediaFogueiras)
            .ThenByDescending(j => j.TotalRecomendacoes)
            .ThenBy(j => j.Nome)
            .Take(quantidade)
            .Select(ProjecaoJogoRespost)
            .ToListAsync(cancellationToken);
    }

    public async Task<JogoCatalogoRespostDto> SalvarOuAtualizarAsync(DetalhesJogoSteamDto detalhesSteam, CancellationToken cancellationToken = default)
    {
        var existente = await context.Jogos
            .FirstOrDefaultAsync(j => j.SteamAppId == detalhesSteam.SteamAppId, cancellationToken);

        if (existente is null)
        {
            existente = new Jogo
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

            await context.Jogos.AddAsync(existente, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var possuiLogger = await context.JogoLoggers
                .AnyAsync(l => l.IdJogo == existente.IdJogo, cancellationToken);

            if (!possuiLogger)
            {
                await context.JogoLoggers.AddAsync(new JogoLogger
                {
                    IdJogo = existente.IdJogo,
                    UltimaAtualizacao = DateTime.UtcNow
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
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
        }

        return new JogoCatalogoRespostDto
        {
            IdJogo = existente.IdJogo,
            SteamAppId = existente.SteamAppId,
            Nome = existente.Nome,
            Generos = existente.Generos.ToList(),
            Descricao = existente.Descricao,
            Imagem = existente.Imagem,
            Desenvolvedora = existente.Desenvolvedora,
            Publicadora = existente.Publicadora,
            DataLancamentoTexto = existente.DataLancamentoTexto,
            AnoLancamento = existente.AnoLancamento,
            NotaMetacritic = existente.NotaMetacritic,
            TotalRecomendacoes = existente.TotalRecomendacoes,
            Gratuito = existente.Gratuito,
            EmBreve = existente.EmBreve,
            MediaFogueiras = existente.MediaFogueiras,
            TotalAvaliacoes = existente.TotalAvaliacoes,
            SincronizadoEm = existente.SincronizadoEm
        };
    }
}
