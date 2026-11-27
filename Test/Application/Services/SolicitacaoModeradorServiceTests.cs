using Application.Dtos.Moderacao.Requests;
using Application.Services;
using Application.Utils;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Model;

namespace Test.Application.Services;

public class SolicitacaoModeradorServiceTests
{
    private sealed class FakeSolicitacaoModeradorRepository : ISolicitacaoModeradorRepository
    {
        public readonly List<Usuario> Usuarios = [];
        public readonly List<Post> Posts = [];
        public readonly List<SolicitacaoModerador> Solicitacoes = [];
        private int _proximoId = 1;

        public Task<Usuario?> ObterUsuarioPorIdAsync(int idUsuario) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == idUsuario));

        public Task<Post?> ObterPostPorIdAsync(int idPost) =>
            Task.FromResult(Posts.FirstOrDefault(p => p.IdPost == idPost));

        public Task<bool> ExisteSolicitacaoAbertaAsync(int idUsuarioSolicitante) =>
            Task.FromResult(Solicitacoes.Any(s =>
                s.IdUsuarioSolicitante == idUsuarioSolicitante &&
                RegrasModeracaoUtil.SolicitacaoEstaAberta(s.Status)));

        public Task<SolicitacaoModerador?> ObterPorIdAsync(int idSolicitacao) =>
            Task.FromResult(Solicitacoes.FirstOrDefault(s => s.Id == idSolicitacao));

        public Task<SolicitacaoModerador?> ObterUltimaPorUsuarioAsync(int idUsuarioSolicitante) =>
            Task.FromResult(Solicitacoes
                .Where(s => s.IdUsuarioSolicitante == idUsuarioSolicitante)
                .OrderByDescending(s => s.DataCriacao)
                .FirstOrDefault());

        public Task<IReadOnlyList<SolicitacaoModerador>> ListarAsync(StatusSolicitacaoModerador? status = null)
        {
            IEnumerable<SolicitacaoModerador> query = Solicitacoes;
            query = status.HasValue
                ? query.Where(s => s.Status == status.Value)
                : query.Where(s => RegrasModeracaoUtil.SolicitacaoEstaAberta(s.Status));

            return Task.FromResult<IReadOnlyList<SolicitacaoModerador>>([.. query.OrderBy(s => s.DataCriacao)]);
        }

        public Task<SolicitacaoModerador> CriarAsync(SolicitacaoModerador solicitacao)
        {
            solicitacao.Id = _proximoId++;
            Solicitacoes.Add(solicitacao);
            return Task.FromResult(solicitacao);
        }

        public Task SalvarAlteracoesAsync() => Task.CompletedTask;
    }

    private static Usuario CriarUsuario(int id, string nome, TipoPerfilUsuario perfil) => new()
    {
        Id = id,
        Nome = nome,
        Email = $"{nome.ToLowerInvariant().Replace(" ", "")}@checkpoint.com",
        Senha = "hash",
        TipoPerfil = perfil
    };

    [Fact]
    public async Task AprovarAsync_AdminAprovaComUmaAprovacao_PromoveUsuarioImediatamente()
    {
        var repo = new FakeSolicitacaoModeradorRepository();
        var regular = CriarUsuario(1, "Jogador Um", TipoPerfilUsuario.Regular);
        var admin = CriarUsuario(2, "Admin Chefe", TipoPerfilUsuario.Admin);
        repo.Usuarios.AddRange([regular, admin]);

        var service = new SolicitacaoModeradorService(repo);
        var criada = await service.SolicitarAsync(regular.Id, new CriarSolicitacaoModeradorRequestDto
        {
            Motivo = "Quero ajudar a moderar reviews da comunidade."
        });

        var aprovada = await service.AprovarAsync(criada.Id, admin.Id);

        Assert.Equal("Aprovada", aprovada.Status);
        Assert.Equal(admin.Id, aprovada.IdPrimeiroAprovador);
        Assert.Equal(TipoPerfilUsuario.Moderador, regular.TipoPerfil);
    }

    [Fact]
    public async Task AprovarAsync_DoisModeradoresDistintos_PromoveNaSegundaAprovacao()
    {
        var repo = new FakeSolicitacaoModeradorRepository();
        var regular = CriarUsuario(1, "Jogador Dois", TipoPerfilUsuario.Regular);
        var mod1 = CriarUsuario(2, "Moderador Um", TipoPerfilUsuario.Moderador);
        var mod2 = CriarUsuario(3, "Moderador Dois", TipoPerfilUsuario.Moderador);
        repo.Usuarios.AddRange([regular, mod1, mod2]);

        var service = new SolicitacaoModeradorService(repo);
        var criada = await service.SolicitarAsync(regular.Id, new CriarSolicitacaoModeradorRequestDto
        {
            Motivo = "Ativo há meses na comunidade e quero ajudar."
        });

        var primeira = await service.AprovarAsync(criada.Id, mod1.Id);
        Assert.Equal("AguardandoSegundaAprovacao", primeira.Status);
        Assert.Equal(TipoPerfilUsuario.Regular, regular.TipoPerfil);

        await Assert.ThrowsAsync<ValidationException>(() => service.AprovarAsync(criada.Id, mod1.Id));

        var segunda = await service.AprovarAsync(criada.Id, mod2.Id);
        Assert.Equal("Aprovada", segunda.Status);
        Assert.Equal(mod1.Id, segunda.IdPrimeiroAprovador);
        Assert.Equal(mod2.Id, segunda.IdSegundoAprovador);
        Assert.Equal(TipoPerfilUsuario.Moderador, regular.TipoPerfil);
    }

    [Fact]
    public async Task SolicitarAsync_JaPossuiSolicitacaoAbertaOuJaEhModerador_LancaConflictException()
    {
        var repo = new FakeSolicitacaoModeradorRepository();
        var regular = CriarUsuario(1, "Jogador", TipoPerfilUsuario.Regular);
        var moderador = CriarUsuario(2, "Mod", TipoPerfilUsuario.Moderador);
        repo.Usuarios.AddRange([regular, moderador]);

        var service = new SolicitacaoModeradorService(repo);
        await service.SolicitarAsync(regular.Id, new CriarSolicitacaoModeradorRequestDto
        {
            Motivo = "Quero colaborar com a moderação do catálogo."
        });

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.SolicitarAsync(regular.Id, new CriarSolicitacaoModeradorRequestDto
            {
                Motivo = "Segunda tentativa duplicada de solicitação."
            }));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.SolicitarAsync(moderador.Id, new CriarSolicitacaoModeradorRequestDto
            {
                Motivo = "Moderador tentando pedir de novo."
            }));
    }

    [Fact]
    public async Task RecusarAsync_ECancelarAsync_RespeitamRegrasDeTransicaoDeEstado()
    {
        var repo = new FakeSolicitacaoModeradorRepository();
        var regular1 = CriarUsuario(1, "Jogador A", TipoPerfilUsuario.Regular);
        var regular2 = CriarUsuario(2, "Jogador B", TipoPerfilUsuario.Regular);
        var mod = CriarUsuario(3, "Moderador", TipoPerfilUsuario.Moderador);
        repo.Usuarios.AddRange([regular1, regular2, mod]);

        var service = new SolicitacaoModeradorService(repo);

        var sol1 = await service.SolicitarAsync(regular1.Id, new CriarSolicitacaoModeradorRequestDto
        {
            Motivo = "Quero ajudar a revisar denúncias de spoilers."
        });

        var recusada = await service.RecusarAsync(sol1.Id, mod.Id, new RecusarSolicitacaoModeradorRequestDto
        {
            Motivo = "Conta ainda muito recente na plataforma."
        });

        Assert.Equal("Recusada", recusada.Status);
        Assert.Equal(mod.Id, recusada.IdAvaliadorRecusa);
        await Assert.ThrowsAsync<ValidationException>(() => service.AprovarAsync(sol1.Id, mod.Id));

        var sol2 = await service.SolicitarAsync(regular2.Id, new CriarSolicitacaoModeradorRequestDto
        {
            Motivo = "Quero ajudar a cuidar das discussões."
        });

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CancelarAsync(sol2.Id, regular1.Id));

        var cancelada = await service.CancelarAsync(sol2.Id, regular2.Id);
        Assert.Equal("Cancelada", cancelada.Status);
    }

    [Fact]
    public async Task RebaixarModeradorAsync_SomenteAdminPodeRebaixarModerador()
    {
        var repo = new FakeSolicitacaoModeradorRepository();
        var admin = CriarUsuario(1, "Admin", TipoPerfilUsuario.Admin);
        var mod1 = CriarUsuario(2, "Mod Um", TipoPerfilUsuario.Moderador);
        var mod2 = CriarUsuario(3, "Mod Dois", TipoPerfilUsuario.Moderador);
        repo.Usuarios.AddRange([admin, mod1, mod2]);

        var service = new SolicitacaoModeradorService(repo);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.RebaixarModeradorAsync(mod1.Id, mod2.Id));
        await Assert.ThrowsAsync<ValidationException>(() => service.RebaixarModeradorAsync(admin.Id, admin.Id));

        await service.RebaixarModeradorAsync(admin.Id, mod1.Id);
        Assert.Equal(TipoPerfilUsuario.Regular, mod1.TipoPerfil);
    }

    [Fact]
    public async Task AlterarSpoilerPostAsync_ModeradorAlteraComSucessoEBloqueiaRegularOuPostDeletado()
    {
        var repo = new FakeSolicitacaoModeradorRepository();
        var regular = CriarUsuario(1, "Jogador", TipoPerfilUsuario.Regular);
        var moderador = CriarUsuario(2, "Moderador", TipoPerfilUsuario.Moderador);
        repo.Usuarios.AddRange([regular, moderador]);

        var postAtivo = new Post
        {
            IdPost = 10,
            IdUsuario = regular.Id,
            IdJogo = 100,
            Titulo = "Final de Elden Ring",
            Fogueiras = 5,
            Conteudo = "No final acontece...",
            TipoPost = [Generos.Acao],
            TemSpoiler = false,
            PostDeletado = false,
            DataCriacao = DateTime.UtcNow
        };

        var postDeletado = new Post
        {
            IdPost = 11,
            IdUsuario = regular.Id,
            IdJogo = 100,
            Titulo = "Post apagado",
            Fogueiras = 3,
            Conteudo = "Conteudo",
            TipoPost = [Generos.Acao],
            TemSpoiler = false,
            PostDeletado = true,
            DataCriacao = DateTime.UtcNow
        };

        repo.Posts.AddRange([postAtivo, postDeletado]);
        var service = new SolicitacaoModeradorService(repo);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.AlterarSpoilerPostAsync(postAtivo.IdPost, regular.Id, new AlterarSpoilerPostRequestDto { TemSpoiler = true }));

        var marcado = await service.AlterarSpoilerPostAsync(
            postAtivo.IdPost,
            moderador.Id,
            new AlterarSpoilerPostRequestDto { TemSpoiler = true });

        Assert.True(marcado.TemSpoiler);
        Assert.True(postAtivo.TemSpoiler);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.AlterarSpoilerPostAsync(postDeletado.IdPost, moderador.Id, new AlterarSpoilerPostRequestDto { TemSpoiler = true }));
    }
}
