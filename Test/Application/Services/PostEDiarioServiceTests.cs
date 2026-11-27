using System.Text.Json;
using Application.Dtos.Diario.Requests;
using Application.Dtos.Posts.Requests;
using Application.Factories;
using Application.Services;
using Application.Strategies.Algoritmos;
using Application.Strategies.Estantes;
using Application.Utils.Token;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Commands;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Diario.Commands;
using Domain.Dtos.Diario.Respost;
using Domain.Dtos.Estantes.Commands;
using Domain.Dtos.Estantes.Respost;
using Domain.Dtos.Loggers.Commands;
using Domain.Dtos.Loggers.Respost;
using Domain.Dtos.Posts.Commands;
using Domain.Dtos.Posts.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Test.Application.Services;

public class PostEDiarioServiceTests
{
    private sealed class EstadoMemoriaCompartilhado
    {
        public readonly Dictionary<int, (string Nome, string? Foto)> Usuarios = new()
        {
            [1] = ("Lucas Andrade", null),
            [2] = ("Mariana Costa", "https://cdn/mari.png")
        };

        public readonly List<JogoCatalogoRespostDto> Jogos = [];
        public readonly Dictionary<(int IdUsuario, int IdJogo), RegistroDiarioRespostDto> Registros = [];
        public readonly List<PostDetalheRespostDto> Posts = [];
        public readonly List<ComentarioRespostDto> Comentarios = [];
        public readonly HashSet<(int IdUsuario, int IdPost)> CurtidasPost = [];
        public readonly HashSet<(int IdUsuario, int IdComentario)> CurtidasComentario = [];
        public readonly Dictionary<int, FavoritosRespostDto> FavoritosPorUsuario = [];
        public readonly Dictionary<int, PretendeJogarRespostDto> PretendePorUsuario = [];
        public readonly Dictionary<int, JogoLoggerRespostDto> JogoLoggers = [];
        public readonly Dictionary<int, UsuarioLoggerRespostDto> UsuarioLoggers = [];
        public readonly Dictionary<Generos, GeneroLoggerRespostDto> GeneroLoggers = [];

        public int ProximoIdJogo = 1;
        public int ProximoIdPost = 1;
        public int ProximoIdComentario = 1;
        public int ProximoIdDiario = 1;
    }

    private sealed class FakeCatalogoRepo(EstadoMemoriaCompartilhado mem) : ICatalogoRepository
    {
        public Task<JogoCatalogoRespostDto?> ObterPorIdAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(mem.Jogos.FirstOrDefault(j => j.IdJogo == idJogo));

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            return Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(mem.Jogos.Where(j => set.Contains(j.IdJogo)).ToList());
        }

        public Task<JogoCatalogoRespostDto?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken ct = default) =>
            Task.FromResult(mem.Jogos.FirstOrDefault(j => j.SteamAppId == steamAppId));

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken ct = default)
        {
            var set = steamAppIds.ToHashSet();
            return Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(mem.Jogos.Where(j => set.Contains(j.SteamAppId)).ToList());
        }

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> BuscarCandidatosLocaisAsync(BuscarCandidatosLocaisCommandDto command, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(mem.Jogos.ToList());

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> ListarCatalogoLocalAsync(int limite = 60, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(mem.Jogos.Take(limite).ToList());

        public Task<JogoCatalogoRespostDto> SalvarOuAtualizarAsync(DetalhesJogoSteamDto d, CancellationToken ct = default)
        {
            var idx = mem.Jogos.FindIndex(j => j.SteamAppId == d.SteamAppId);
            if (idx >= 0)
            {
                return Task.FromResult(mem.Jogos[idx]);
            }

            var novo = new JogoCatalogoRespostDto
            {
                IdJogo = mem.ProximoIdJogo++,
                SteamAppId = d.SteamAppId,
                Nome = d.Nome,
                Descricao = d.DescricaoCurta,
                Imagem = d.ImagemCapaUrl,
                Desenvolvedora = d.Desenvolvedora,
                Publicadora = d.Publicadora,
                Generos = d.Generos.ToList(),
                AnoLancamento = d.AnoLancamento,
                NotaMetacritic = d.NotaMetacritic,
                TotalRecomendacoes = d.TotalRecomendacoes,
                Gratuito = d.Gratuito,
                EmBreve = d.EmBreve
            };
            mem.Jogos.Add(novo);
            return Task.FromResult(novo);
        }
    }

    private sealed class FakeEstanteRepo(EstadoMemoriaCompartilhado mem) : IEstanteRepository
    {
        public Task<FavoritosRespostDto?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(mem.FavoritosPorUsuario.TryGetValue(idUsuario, out var f) ? f : null);

        public Task<PretendeJogarRespostDto?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(mem.PretendePorUsuario.TryGetValue(idUsuario, out var p) ? p : null);

        public Task<FavoritosRespostDto> SalvarFavoritosAsync(SalvarFavoritosCommandDto command, CancellationToken ct = default)
        {
            var dto = new FavoritosRespostDto
            {
                IdCatalogo = 1,
                IdUsuario = command.IdUsuario,
                Jogos = command.Jogos.ToList(),
                JogoDestaquePerfilId = command.JogoDestaquePerfilId,
                ExibirNoTopoPerfil = command.ExibirNoTopoPerfil,
                ObservacaoDestaque = command.ObservacaoDestaque,
                DataAtualizacao = DateTime.UtcNow
            };
            mem.FavoritosPorUsuario[command.IdUsuario] = dto;
            return Task.FromResult(dto);
        }

        public Task<PretendeJogarRespostDto> SalvarPretendeJogarAsync(SalvarPretendeJogarCommandDto command, CancellationToken ct = default)
        {
            var dto = new PretendeJogarRespostDto
            {
                IdCatalogo = 2,
                IdUsuario = command.IdUsuario,
                Jogos = command.Jogos.ToList(),
                NivelPrioridade = command.NivelPrioridade,
                NotificarEmPromocao = command.NotificarEmPromocao,
                MetaAnoConclusao = command.MetaAnoConclusao,
                DataAtualizacao = DateTime.UtcNow
            };
            mem.PretendePorUsuario[command.IdUsuario] = dto;
            return Task.FromResult(dto);
        }
    }

    private sealed class FakeLoggerRepo(EstadoMemoriaCompartilhado mem) : IEngajamentoLoggerRepository
    {
        public Task<IReadOnlyList<GeneroLoggerRespostDto>> ObterGeneroLoggersAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<GeneroLoggerRespostDto>>(mem.GeneroLoggers.Values.ToList());

        public Task<JogoLoggerRespostDto?> ObterJogoLoggerAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(mem.JogoLoggers.TryGetValue(idJogo, out var l) ? l : null);

        public Task<IReadOnlyDictionary<int, JogoLoggerRespostDto>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            IReadOnlyDictionary<int, JogoLoggerRespostDto> dict = mem.JogoLoggers
                .Where(kv => set.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return Task.FromResult(dict);
        }

        public Task AplicarImpactoEstanteAsync(ImpactoEngajamentoEstanteCommandDto command, CancellationToken ct = default)
        {
            var atual = mem.JogoLoggers.TryGetValue(command.IdJogo, out var jl)
                ? jl
                : new JogoLoggerRespostDto { IdJogo = command.IdJogo };

            mem.JogoLoggers[command.IdJogo] = atual with
            {
                PresencaFavoritos = (ulong)Math.Max(0, (int)atual.PresencaFavoritos + command.DeltaFavoritosJogo),
                PresencaPretendeJogar = (ulong)Math.Max(0, (int)atual.PresencaPretendeJogar + command.DeltaPretendeJogarJogo)
            };
            return Task.CompletedTask;
        }

        public Task SincronizarAvaliacoesJogoEUsuarioAsync(
            int idJogo,
            int idUsuario,
            IReadOnlyList<Generos> generosJogo,
            int deltaPostsGenero = 0,
            CancellationToken ct = default)
        {
            var mapaNotas = new Dictionary<int, double>();
            foreach (var reg in mem.Registros.Values.Where(r => r.IdJogo == idJogo && r.NotaFogueiras.HasValue))
            {
                mapaNotas[reg.IdUsuario] = reg.NotaFogueiras!.Value;
            }
            foreach (var post in mem.Posts.Where(p => p.IdJogo == idJogo))
            {
                if (!mapaNotas.ContainsKey(post.IdUsuario))
                {
                    mapaNotas[post.IdUsuario] = post.Fogueiras;
                }
            }

            var soma = Math.Round(mapaNotas.Values.Sum(), 2);
            var total = mapaNotas.Count;
            var media = total > 0 ? Math.Round(soma / total, 2) : 0.0;
            var totalPosts = mem.Posts.Count(p => p.IdJogo == idJogo);

            var idxJogo = mem.Jogos.FindIndex(j => j.IdJogo == idJogo);
            if (idxJogo >= 0)
            {
                mem.Jogos[idxJogo] = mem.Jogos[idxJogo] with
                {
                    MediaFogueiras = media,
                    TotalAvaliacoes = total
                };
            }

            var atualLogger = mem.JogoLoggers.TryGetValue(idJogo, out var jl)
                ? jl
                : new JogoLoggerRespostDto { IdJogo = idJogo };

            mem.JogoLoggers[idJogo] = atualLogger with
            {
                PresencaPost = (ulong)totalPosts,
                SomaFogueiras = soma,
                TotalAvaliacoes = (ulong)total,
                MediaFogueiras = media
            };

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UsuarioPopularComunidadeDto>>([]);
    }

    private sealed class FakeDiarioRepo(EstadoMemoriaCompartilhado mem) : IDiarioRepository
    {
        public Task<IReadOnlyList<RegistroDiarioRespostDto>> ListarPorUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var favs = mem.FavoritosPorUsuario.TryGetValue(idUsuario, out var f) ? f.Jogos.ToHashSet() : [];
            var pret = mem.PretendePorUsuario.TryGetValue(idUsuario, out var p) ? p.Jogos.ToHashSet() : [];

            var itens = mem.Registros.Values
                .Where(r => r.IdUsuario == idUsuario)
                .Select(r =>
                {
                    var jogo = mem.Jogos.First(j => j.IdJogo == r.IdJogo);
                    var post = mem.Posts.FirstOrDefault(po => po.IdUsuario == idUsuario && po.IdJogo == r.IdJogo);
                    return r with
                    {
                        MediaFogueirasJogo = jogo.MediaFogueiras,
                        TotalAvaliacoesJogo = jogo.TotalAvaliacoes,
                        Favorito = favs.Contains(r.IdJogo),
                        QueroJogar = pret.Contains(r.IdJogo) || r.Status == StatusJornada.QueroJogar,
                        IdPostReview = post?.IdPost,
                        ComentarioReview = post?.Conteudo,
                        ContemSpoilerReview = post?.TemSpoiler ?? false
                    };
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<RegistroDiarioRespostDto>>(itens);
        }

        public async Task<RegistroDiarioRespostDto?> ObterPorUsuarioEJogoAsync(int idUsuario, int idJogo, CancellationToken ct = default)
        {
            var todos = await ListarPorUsuarioAsync(idUsuario, ct);
            return todos.FirstOrDefault(r => r.IdJogo == idJogo);
        }

        public async Task<RegistroDiarioRespostDto> SalvarOuAtualizarAsync(SalvarAvaliacaoDiarioCommandDto command, CancellationToken ct = default)
        {
            var jogo = mem.Jogos.First(j => j.IdJogo == command.IdJogo);
            mem.Registros.TryGetValue((command.IdUsuario, command.IdJogo), out var anterior);

            var jogado = command.Jogado
                ?? (command.Status is StatusJornada.Zerado or StatusJornada.Jogando || (anterior?.Jogado ?? false));

            var salvo = new RegistroDiarioRespostDto
            {
                IdRegistroDiario = anterior?.IdRegistroDiario ?? mem.ProximoIdDiario++,
                IdUsuario = command.IdUsuario,
                IdJogo = jogo.IdJogo,
                SteamAppId = jogo.SteamAppId,
                NomeJogo = jogo.Nome,
                ImagemJogo = jogo.Imagem,
                AnoLancamentoJogo = jogo.AnoLancamento,
                DesenvolvedoraJogo = jogo.Desenvolvedora,
                GenerosJogo = jogo.Generos,
                MediaFogueirasJogo = jogo.MediaFogueiras,
                TotalAvaliacoesJogo = jogo.TotalAvaliacoes,
                Jogado = jogado,
                Status = command.Status ?? anterior?.Status,
                NotaFogueiras = command.NotaFogueiras ?? anterior?.NotaFogueiras,
                Tags = command.Tags ?? anterior?.Tags ?? [],
                DataAtualizacao = DateTime.UtcNow
            };

            mem.Registros[(command.IdUsuario, command.IdJogo)] = salvo;
            return (await ObterPorUsuarioEJogoAsync(command.IdUsuario, command.IdJogo, ct))!;
        }
    }

    private sealed class FakePostRepo(EstadoMemoriaCompartilhado mem) : IPostRepository
    {
        private PostDetalheRespostDto Hidratar(PostDetalheRespostDto p, int? idUsuarioAtual) => p with
        {
            CurtidoPeloUsuarioAtual = idUsuarioAtual.HasValue && mem.CurtidasPost.Contains((idUsuarioAtual.Value, p.IdPost)),
            DoUsuarioAtual = idUsuarioAtual.HasValue && idUsuarioAtual.Value == p.IdUsuario,
            QuantidadeCurtidas = mem.CurtidasPost.Count(c => c.IdPost == p.IdPost),
            QuantidadeComentarios = mem.Comentarios.Count(c => c.IdPost == p.IdPost)
        };

        public Task<PostDetalheRespostDto?> ObterPorIdAsync(int idPost, int? idUsuarioAtual = null, CancellationToken ct = default)
        {
            var p = mem.Posts.FirstOrDefault(x => x.IdPost == idPost);
            return Task.FromResult(p is null ? null : Hidratar(p, idUsuarioAtual));
        }

        public Task<IReadOnlyList<PostDetalheRespostDto>> ListarRecentesAsync(int limite = 20, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PostDetalheRespostDto>>(
                mem.Posts.OrderByDescending(p => p.DataCriacao).Take(limite).Select(p => Hidratar(p, idUsuarioAtual)).ToList());

        public Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorJogoAsync(int idJogo, string? ordenacao = null, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PostDetalheRespostDto>>(
                mem.Posts.Where(p => p.IdJogo == idJogo).Select(p => Hidratar(p, idUsuarioAtual)).ToList());

        public Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorUsuarioAsync(int idUsuario, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PostDetalheRespostDto>>(
                mem.Posts.Where(p => p.IdUsuario == idUsuario).Select(p => Hidratar(p, idUsuarioAtual)).ToList());

        public Task<(PostDetalheRespostDto Post, bool FoiCriadoNovo)> SalvarOuAtualizarDoUsuarioNoJogoAsync(SalvarPostCommandDto command, CancellationToken ct = default)
        {
            var jogo = mem.Jogos.First(j => j.IdJogo == command.IdJogo);
            var usuario = mem.Usuarios[command.IdUsuario];
            var idx = mem.Posts.FindIndex(p => p.IdUsuario == command.IdUsuario && p.IdJogo == command.IdJogo);

            if (idx >= 0)
            {
                var atualizado = mem.Posts[idx] with
                {
                    Titulo = command.Titulo ?? mem.Posts[idx].Titulo,
                    Fogueiras = command.Fogueiras,
                    Conteudo = command.Conteudo.Trim(),
                    StatusJornada = command.StatusJornada,
                    Tags = command.Tags,
                    TemSpoiler = command.TemSpoiler,
                    DataAtualizacao = DateTime.UtcNow
                };
                mem.Posts[idx] = atualizado;
                return Task.FromResult((Hidratar(atualizado, command.IdUsuario), false));
            }

            var novo = new PostDetalheRespostDto
            {
                IdPost = mem.ProximoIdPost++,
                IdUsuario = command.IdUsuario,
                NomeAutor = usuario.Nome,
                FotoPerfilAutor = usuario.Foto,
                IdJogo = jogo.IdJogo,
                SteamAppId = jogo.SteamAppId,
                NomeJogo = jogo.Nome,
                AnoLancamentoJogo = jogo.AnoLancamento,
                ImagemJogo = jogo.Imagem,
                DesenvolvedoraJogo = jogo.Desenvolvedora,
                Titulo = command.Titulo ?? "Relato na Fogueira",
                Fogueiras = command.Fogueiras,
                Conteudo = command.Conteudo.Trim(),
                StatusJornada = command.StatusJornada,
                Tags = command.Tags,
                TemSpoiler = command.TemSpoiler,
                DoUsuarioAtual = true,
                DataCriacao = DateTime.UtcNow
            };
            mem.Posts.Add(novo);
            return Task.FromResult((Hidratar(novo, command.IdUsuario), true));
        }

        public Task<bool> ExcluirAsync(int idPost, int idUsuarioSolicitante, bool podeModerar = false, CancellationToken ct = default)
        {
            var p = mem.Posts.FirstOrDefault(x => x.IdPost == idPost)
                ?? throw new NotFoundException("Review não encontrada.");

            if (p.IdUsuario != idUsuarioSolicitante && !podeModerar)
            {
                throw new ForbiddenException("Você só pode excluir suas próprias reviews.");
            }

            mem.Posts.Remove(p);
            return Task.FromResult(true);
        }

        public Task<ResultadoCurtidaRespostDto> AlternarCurtidaPostAsync(int idPost, int idUsuario, CancellationToken ct = default)
        {
            var chave = (idUsuario, idPost);
            var curtido = mem.CurtidasPost.Add(chave);
            if (!curtido)
            {
                mem.CurtidasPost.Remove(chave);
            }

            var total = mem.CurtidasPost.Count(c => c.IdPost == idPost);
            return Task.FromResult(new ResultadoCurtidaRespostDto
            {
                IdAlvo = idPost,
                Curtido = curtido,
                QuantidadeCurtidas = total
            });
        }

        public Task<IReadOnlyList<ComentarioRespostDto>> ListarComentariosPorPostAsync(int idPost, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ComentarioRespostDto>>(
                mem.Comentarios.Where(c => c.IdPost == idPost).OrderByDescending(c => c.DataCriacao).ToList());

        public Task<ComentarioRespostDto> AdicionarComentarioAsync(CriarComentarioCommandDto command, CancellationToken ct = default)
        {
            var usuario = mem.Usuarios[command.IdUsuario];
            var novo = new ComentarioRespostDto
            {
                IdComentario = mem.ProximoIdComentario++,
                IdPost = command.IdPost,
                IdUsuario = command.IdUsuario,
                NomeAutor = usuario.Nome,
                FotoPerfilAutor = usuario.Foto,
                Conteudo = command.Conteudo.Trim(),
                TemSpoiler = command.TemSpoiler,
                DoUsuarioAtual = true,
                DataCriacao = DateTime.UtcNow
            };
            mem.Comentarios.Add(novo);
            return Task.FromResult(novo);
        }

        public Task<ResultadoCurtidaRespostDto> AlternarCurtidaComentarioAsync(int idComentario, int idUsuario, CancellationToken ct = default)
        {
            var chave = (idUsuario, idComentario);
            var curtido = mem.CurtidasComentario.Add(chave);
            if (!curtido)
            {
                mem.CurtidasComentario.Remove(chave);
            }

            return Task.FromResult(new ResultadoCurtidaRespostDto
            {
                IdAlvo = idComentario,
                Curtido = curtido,
                QuantidadeCurtidas = mem.CurtidasComentario.Count(c => c.IdComentario == idComentario)
            });
        }

        public Task<IReadOnlyList<FaixaDistribuicaoFogueirasDto>> ObterDistribuicaoFogueirasJogoAsync(int idJogo, CancellationToken ct = default)
        {
            var notas = mem.Registros.Values
                .Where(r => r.IdJogo == idJogo && r.NotaFogueiras.HasValue)
                .Select(r => r.NotaFogueiras!.Value)
                .ToList();

            var total = notas.Count;
            var faixas = new[] { 5, 4, 3, 2, 1 }
                .Select(e =>
                {
                    var qtd = notas.Count(n => Math.Clamp((int)Math.Ceiling(n), 1, 5) == e);
                    return new FaixaDistribuicaoFogueirasDto
                    {
                        Estrelas = e,
                        Quantidade = qtd,
                        Percentual = total > 0 ? (int)Math.Round(qtd * 100.0 / total) : 0
                    };
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<FaixaDistribuicaoFogueirasDto>>(faixas);
        }
    }

    private sealed class StubSteamService : ISteamStoreService
    {
        public readonly Dictionary<int, DetalhesJogoSteamDto> Jogos = [];

        public Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarJogosAsync(string jogoBuscado, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ItemBuscaSteamDto>>([]);

        public Task<DestaquesSteamDto> ObterDestaquesAsync(CancellationToken ct = default) =>
            Task.FromResult(new DestaquesSteamDto
            {
                MaisVendidos = [],
                Lancamentos = [],
                OfertasEspeciais = [],
                EmBreve = []
            });

        public Task<DetalhesJogoSteamDto?> ObterDetalhesJogoAsync(int steamAppId, CancellationToken ct = default) =>
            Task.FromResult(Jogos.TryGetValue(steamAppId, out var d) ? d : null);
    }

    private sealed class StubCacheService : ICatalogoCacheService
    {
        private readonly Dictionary<string, object> _mem = [];

        public Task<T?> ObterAsync<T>(string chave, CancellationToken ct = default) =>
            Task.FromResult(_mem.TryGetValue(chave, out var v) && v is T t ? t : default);

        public Task SalvarAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken ct = default)
        {
            if (valor is not null) _mem[chave] = valor;
            return Task.CompletedTask;
        }

        public Task RemoverAsync(string chave, CancellationToken ct = default)
        {
            _mem.Remove(chave);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task FluxoCompleto_AvaliacaoMeiaChamaMediaFogueirasComentariosSpoilerECurtidas()
    {
        var mem = new EstadoMemoriaCompartilhado();
        var steam = new StubSteamService();
        steam.Jogos[1245620] = new DetalhesJogoSteamDto
        {
            SteamAppId = 1245620,
            Nome = "Elden Ring",
            DescricaoCurta = "Ergue-te, Maculado.",
            ImagemCapaUrl = "https://steam/1245620.jpg",
            Desenvolvedora = "FromSoftware",
            Publicadora = "Bandai Namco",
            Generos = [Generos.Rpg, Generos.Acao],
            AnoLancamento = 2022,
            NotaMetacritic = 96,
            TotalRecomendacoes = 600000
        };

        var cache = new StubCacheService();
        var catalogoRepo = new FakeCatalogoRepo(mem);
        var estanteRepo = new FakeEstanteRepo(mem);
        var loggerRepo = new FakeLoggerRepo(mem);
        var postRepo = new FakePostRepo(mem);
        var diarioRepo = new FakeDiarioRepo(mem);
        var regraEstanteFactory = new RegraEstanteFactory([new RegraEstanteFavoritos(), new RegraEstantePretendeJogar()]);
        var algoritmoFactory = new AlgoritmoCuradoriaFactory([new AlgoritmoPopularidadeJogo(), new AlgoritmoTendenciaGenero(), new AlgoritmoCustoBeneficio()]);

        var diarioService = new DiarioService(
            diarioRepo,
            postRepo,
            estanteRepo,
            catalogoRepo,
            loggerRepo,
            steam,
            cache,
            regraEstanteFactory);

        var postService = new PostService(
            postRepo,
            diarioRepo,
            catalogoRepo,
            loggerRepo,
            steam,
            cache);

        var vitrineService = new VitrineService(
            catalogoRepo,
            postRepo,
            loggerRepo,
            steam,
            cache,
            algoritmoFactory);

        // 1. Usuário 1 avalia Elden Ring com 4.5 chamas, favorito, tags e review com spoiler
        var avaliacao1 = await diarioService.SalvarAvaliacaoJogoAsync(1, 1245620, new SalvarAvaliacaoJogoRequestDto
        {
            Nota = 4.5,
            Status = "zerado",
            Favorito = true,
            Tags = ["Trilha sonora", "Desafiador"],
            Comentario = "A luta contra a Malenia é inesquecível.",
            ContemSpoiler = true
        });

        Assert.Equal(4.5, avaliacao1.MediaFogueirasJogo);
        Assert.Equal(1, avaliacao1.TotalAvaliacoesJogo);
        Assert.True(avaliacao1.Registro.Favorito);
        Assert.True(avaliacao1.Registro.Jogado);
        Assert.Equal("zerado", avaliacao1.Registro.Status);
        Assert.NotNull(avaliacao1.ReviewPublicada);
        Assert.True(avaliacao1.ReviewPublicada!.ContemSpoiler);
        Assert.Contains("Trilha sonora", avaliacao1.ReviewPublicada.Tags);

        // 2. Usuário 2 avalia apenas com nota 3.5 sem review escrita -> média vira (4.5 + 3.5) / 2 = 4.0
        var avaliacao2 = await diarioService.SalvarAvaliacaoJogoAsync(2, 1245620, new SalvarAvaliacaoJogoRequestDto
        {
            Nota = 3.5,
            Status = "jogando"
        });

        Assert.Equal(4.0, avaliacao2.MediaFogueirasJogo);
        Assert.Equal(2, avaliacao2.TotalAvaliacoesJogo);
        Assert.Null(avaliacao2.ReviewPublicada);

        // 3. Usuário 1 edita sua avaliação para 5.0 -> não duplica post nem contagem; média vira (5.0 + 3.5) / 2 = 4.25
        var avaliacao1Editada = await diarioService.SalvarAvaliacaoJogoAsync(1, 1245620, new SalvarAvaliacaoJogoRequestDto
        {
            Nota = 5.0,
            Status = "zerado",
            Favorito = true,
            Tags = ["História marcante"],
            Comentario = "140 horas depois, obra-prima absoluta!",
            ContemSpoiler = false
        });

        Assert.Equal(4.25, avaliacao1Editada.MediaFogueirasJogo);
        Assert.Equal(2, avaliacao1Editada.TotalAvaliacoesJogo);
        Assert.Equal(avaliacao1.ReviewPublicada.IdPost, avaliacao1Editada.ReviewPublicada!.IdPost);

        // 4. Usuário 2 curte e comenta com spoiler na review do Usuário 1
        var idPost = avaliacao1Editada.ReviewPublicada.IdPost;
        var curtida = await postService.AlternarCurtidaPostAsync(idPost, 2);
        Assert.True(curtida.Curtido);
        Assert.Equal(1, curtida.QuantidadeCurtidas);

        var comentario = await postService.AdicionarComentarioAsync(idPost, 2, new CriarComentarioRequestDto
        {
            Texto = "O final da Ranni muda tudo!",
            ContemSpoiler = true
        });
        Assert.True(comentario.ContemSpoiler);
        Assert.Equal("Mariana Costa", comentario.AutorNome);

        var detalhesPost = await postService.ObterDetalhesComComentariosAsync(idPost, 2);
        Assert.True(detalhesPost.Review.CurtidaPeloUsuario);
        Assert.Single(detalhesPost.Comentarios);
        Assert.True(detalhesPost.Comentarios[0].ContemSpoiler);

        // 5. Vitrine Home retorna o jogo com a média de fogueiras atualizada e a review na seção Na Fogueira Agora
        var vitrine = await vitrineService.ObterVitrineHomeAsync(2);
        Assert.NotEmpty(vitrine.Destaques);
        Assert.Equal(4.25, vitrine.Destaques[0].MediaFogueiras);
        var secaoReviews = vitrine.Secoes.Single(s => s.Tipo == "REVIEWS");
        Assert.Single(secaoReviews.Reviews);
        Assert.Equal(idPost, secaoReviews.Reviews[0].IdPost);

        // 6. Usuário 2 não pode excluir a review do Usuário 1
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            postService.ExcluirAsync(idPost, 2, "Regular"));
    }

    [Fact]
    public void Token_IncluiClaimUniqueNameQuandoNomeInformado()
    {
        const string segredo = "chave-secreta-de-teste-com-mais-de-32-caracteres!";
        var jwt = Token.GerarToken(7, "bruno@checkpoint.gg", "Regular", segredo, nome: "Bruno Diniz");

        Assert.True(Token.ValidarToken(jwt, segredo));

        var payloadPart = jwt.Split('.')[1];
        var padded = payloadPart.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');
        using var doc = JsonDocument.Parse(Convert.FromBase64String(padded));

        Assert.Equal("Bruno Diniz", doc.RootElement.GetProperty("unique_name").GetString());
    }
}
