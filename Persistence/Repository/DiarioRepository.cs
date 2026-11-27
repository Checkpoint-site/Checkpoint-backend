using Domain.Dtos.Diario.Commands;
using Domain.Dtos.Diario.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class DiarioRepository(ApplicationDbContext context) : IDiarioRepository
{
    public async Task<IReadOnlyList<RegistroDiarioRespostDto>> ListarPorUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var registros = await context.RegistrosDiario
            .AsNoTracking()
            .Where(r => r.IdUsuario == idUsuario)
            .ToListAsync(cancellationToken);

        var favoritos = await context.Favoritos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.IdUsuario == idUsuario, cancellationToken);

        var pretendeJogar = await context.PretendeJogar
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario, cancellationToken);

        var postsUsuario = await context.Posts
            .AsNoTracking()
            .Where(p => p.IdUsuario == idUsuario && !p.PostDeletado)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync(cancellationToken);

        var setFavoritos = favoritos?.Jogos.ToHashSet() ?? [];
        var setPretende = pretendeJogar?.Jogos.ToHashSet() ?? [];
        var mapaRegistros = registros.ToDictionary(r => r.IdJogo);
        var mapaPosts = postsUsuario
            .GroupBy(p => p.IdJogo)
            .ToDictionary(g => g.Key, g => g.First());

        var todosIdsJogos = mapaRegistros.Keys
            .Concat(setFavoritos)
            .Concat(setPretende)
            .Concat(mapaPosts.Keys)
            .Distinct()
            .ToList();

        if (todosIdsJogos.Count == 0)
        {
            return [];
        }

        var jogos = await context.Jogos
            .AsNoTracking()
            .Where(j => todosIdsJogos.Contains(j.IdJogo))
            .ToDictionaryAsync(j => j.IdJogo, cancellationToken);

        var resultado = new List<RegistroDiarioRespostDto>();

        foreach (var idJogo in todosIdsJogos)
        {
            if (!jogos.TryGetValue(idJogo, out var jogo))
            {
                continue;
            }

            mapaRegistros.TryGetValue(idJogo, out var reg);
            mapaPosts.TryGetValue(idJogo, out var post);

            var queroJogar = setPretende.Contains(idJogo) || reg?.Status == StatusJornada.QueroJogar;
            var favorito = setFavoritos.Contains(idJogo);
            var status = reg?.Status ?? post?.StatusJornada ?? (queroJogar ? StatusJornada.QueroJogar : null);
            var jogado = reg?.Jogado ?? (post is not null || status is StatusJornada.Zerado or StatusJornada.Jogando);
            var nota = reg?.NotaFogueiras ?? post?.Fogueiras;
            var tags = reg is { Tags.Count: > 0 } ? reg.Tags : (post?.Tags ?? []);
            var atualizadoEm = reg?.DataAtualizacao
                ?? post?.DataAtualizacao
                ?? post?.DataCriacao
                ?? favoritos?.DataAtualizacao
                ?? pretendeJogar?.DataAtualizacao
                ?? DateTime.UtcNow;

            resultado.Add(new RegistroDiarioRespostDto
            {
                IdRegistroDiario = reg?.IdRegistroDiario ?? 0,
                IdUsuario = idUsuario,
                IdJogo = jogo.IdJogo,
                SteamAppId = jogo.SteamAppId,
                NomeJogo = jogo.Nome,
                ImagemJogo = jogo.Imagem,
                AnoLancamentoJogo = jogo.AnoLancamento,
                DesenvolvedoraJogo = jogo.Desenvolvedora,
                GenerosJogo = jogo.Generos.ToList(),
                MediaFogueirasJogo = jogo.MediaFogueiras,
                TotalAvaliacoesJogo = jogo.TotalAvaliacoes,
                Jogado = jogado,
                QueroJogar = queroJogar,
                Favorito = favorito,
                Status = status,
                NotaFogueiras = nota,
                Tags = tags,
                IdPostReview = post?.IdPost,
                ComentarioReview = post?.Conteudo,
                ContemSpoilerReview = post?.TemSpoiler ?? false,
                DataAtualizacao = atualizadoEm
            });
        }

        return resultado
            .OrderByDescending(r => r.DataAtualizacao)
            .ToList();
    }

    public async Task<RegistroDiarioRespostDto?> ObterPorUsuarioEJogoAsync(
        int idUsuario,
        int idJogo,
        CancellationToken cancellationToken = default)
    {
        var todos = await ListarPorUsuarioAsync(idUsuario, cancellationToken);
        return todos.FirstOrDefault(r => r.IdJogo == idJogo);
    }

    public async Task<RegistroDiarioRespostDto> SalvarOuAtualizarAsync(
        SalvarAvaliacaoDiarioCommandDto command,
        CancellationToken cancellationToken = default)
    {
        var jogoExiste = await context.Jogos
            .AsNoTracking()
            .AnyAsync(j => j.IdJogo == command.IdJogo, cancellationToken);

        if (!jogoExiste)
        {
            throw new NotFoundException("Jogo não encontrado no catálogo.");
        }

        var registro = await context.RegistrosDiario
            .FirstOrDefaultAsync(r => r.IdUsuario == command.IdUsuario && r.IdJogo == command.IdJogo, cancellationToken);

        if (registro is null)
        {
            registro = new RegistroDiario
            {
                IdUsuario = command.IdUsuario,
                IdJogo = command.IdJogo,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };
            AplicarDadosRegistro(registro, command);
            await context.RegistrosDiario.AddAsync(registro, cancellationToken);
        }
        else
        {
            AplicarDadosRegistro(registro, command);
        }

        await context.SaveChangesAsync(cancellationToken);

        return await ObterPorUsuarioEJogoAsync(command.IdUsuario, command.IdJogo, cancellationToken)
            ?? throw new NotFoundException("Registro do diário não pôde ser recuperado após salvar.");
    }

    private static void AplicarDadosRegistro(RegistroDiario registro, SalvarAvaliacaoDiarioCommandDto command)
    {
        if (command.NotaFogueiras.HasValue)
        {
            registro.NotaFogueiras = Math.Round(command.NotaFogueiras.Value * 2.0) / 2.0;
        }

        if (command.Status.HasValue)
        {
            registro.Status = command.Status.Value;
        }

        if (command.Jogado.HasValue)
        {
            registro.Jogado = command.Jogado.Value;
        }
        else if (command.Status is StatusJornada.Zerado or StatusJornada.Jogando)
        {
            registro.Jogado = true;
        }

        if (command.Tags is not null)
        {
            registro.Tags = command.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Where(t => t.Length <= 40)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();
        }

        registro.DataAtualizacao = DateTime.UtcNow;
    }
}
