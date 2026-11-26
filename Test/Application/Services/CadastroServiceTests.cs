using Application.Dtos.Cadastro.Requests;
using Application.Interfaces;
using Application.Services;
using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Response;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Xunit;

namespace Test.Application.Services;

[Collection("Sequential")]
public class CadastroServiceTests : IDisposable
{
    private const string ChaveSecretaTeste = "ChaveSecretaSuperSeguraParaTestesComMaisDe32Caracteres!";

    public CadastroServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveSecretaTeste);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
    }

    private sealed class FakeCadastroRepository : ICadastroRepository
    {
        private readonly List<CadastroCommandsDto> _usuarios = [];
        private int _proximoId = 1;

        public Task<bool> EmailExisteAsync(string email)
        {
            var existe = _usuarios.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(existe);
        }

        public Task<CadastroRespostDto?> CadastroRepositoryAsync(CadastroCommandsDto command)
        {
            if (_usuarios.Any(u => u.Email.Equals(command.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult<CadastroRespostDto?>(null);
            }

            _usuarios.Add(command);

            var resposta = new CadastroRespostDto
            {
                Id = _proximoId++,
                Nome = command.Nome,
                Email = command.Email,
                TipoPerfil = command.TipoPerfil
            };

            return Task.FromResult<CadastroRespostDto?>(resposta);
        }
    }

    private sealed class FakeCodigoVerificacaoService : ICodigoVerificacaoService
    {
        public readonly Dictionary<string, string> Codigos = new(StringComparer.OrdinalIgnoreCase);

        public Task SalvarCodigoAsync(string email, string codigo, TimeSpan expiracao)
        {
            Codigos[email] = codigo;
            return Task.CompletedTask;
        }

        public Task<string?> ObterCodigoAsync(string email)
        {
            Codigos.TryGetValue(email, out var codigo);
            return Task.FromResult(codigo);
        }

        public Task RemoverCodigoAsync(string email)
        {
            Codigos.Remove(email);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmailService : IEmailService
    {
        public readonly List<(string Destinatario, string Assunto, string Corpo)> EmailsEnviados = [];

        public Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            EmailsEnviados.Add((destinatario, assunto, corpo));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeModeracaoFotoService : IModeracaoFotoService
    {
        public bool FotoSegura { get; set; } = true;

        public Task<bool> FotoEhSeguraAsync(string? urlOuBase64) => Task.FromResult(FotoSegura);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_EmailLivre_RetornaDisponivelTrue()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        var request = new VerificarEmailRequestDto { Email = "novo@jogador.com" };

        var resultado = await service.VerificarDisponibilidadeAsync(request);

        Assert.NotNull(resultado);
        Assert.True(resultado.Disponivel);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_EmailEmUso_RetornaDisponivelFalse()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["existente@jogador.com"] = "123456";
        await service.CadastrarAsync(new CadastroRequestDto
        {
            Nome = "Existente",
            Email = "existente@jogador.com",
            Senha = "SenhaSegura123!",
            Codigo = "123456"
        });

        var resultado = await service.VerificarDisponibilidadeAsync(new VerificarEmailRequestDto
        {
            Email = "existente@jogador.com"
        });

        Assert.NotNull(resultado);
        Assert.False(resultado.Disponivel);
    }

    [Fact]
    public async Task SolicitarCodigoAsync_EmailDisponivel_GeraCodigoEnviaEmailERetornaSucesso()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        var request = new SolicitarCodigoRequestDto { Email = "arthur@van-der-linde.com" };

        var resultado = await service.SolicitarCodigoAsync(request);

        Assert.NotNull(resultado);
        Assert.Equal("Código de verificação enviado com sucesso.", resultado.Mensagem);
        Assert.True(fakeCodigo.Codigos.ContainsKey("arthur@van-der-linde.com"));
        Assert.Single(fakeEmail.EmailsEnviados);
    }

    [Fact]
    public async Task SolicitarCodigoAsync_EmailJaCadastrado_LancaConflictException()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["arthur@van-der-linde.com"] = "111111";
        await service.CadastrarAsync(new CadastroRequestDto
        {
            Nome = "Arthur Morgan",
            Email = "arthur@van-der-linde.com",
            Senha = "SenhaSegura123!",
            Codigo = "111111"
        });

        var request = new SolicitarCodigoRequestDto { Email = "arthur@van-der-linde.com" };

        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.SolicitarCodigoAsync(request));
        Assert.Equal("Este e-mail já está em uso.", ex.Message);
    }

    [Fact]
    public async Task CadastrarAsync_CodigoCorretoEDadosValidos_CadastraComSucessoERetornaToken()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["john@beechershope.com"] = "654321";

        var request = new CadastroRequestDto
        {
            Nome = "John Marston",
            Email = "john@beechershope.com",
            Senha = "SenhaSegura123!",
            Codigo = "654321"
        };

        var resultado = await service.CadastrarAsync(request);

        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.False(fakeCodigo.Codigos.ContainsKey("john@beechershope.com"));
    }

    [Fact]
    public async Task CadastrarAsync_ModeradorComDadosValidos_CadastraComSucesso()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["mod@checkpoint.com"] = "999888";

        var request = new CadastroRequestDto
        {
            Nome = "Moderador Geral",
            Email = "mod@checkpoint.com",
            Senha = "SenhaSegura123!",
            Codigo = "999888",
            TipoPerfil = "Moderador",
            CodigoCredencial = "MOD-999",
            SetorModeracao = "Geral"
        };

        var resultado = await service.CadastrarAsync(request);

        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
    }

    [Fact]
    public async Task CadastrarAsync_ModeradorSemCredencial_LancaValidationException()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["mod@checkpoint.com"] = "999888";

        var request = new CadastroRequestDto
        {
            Nome = "Moderador Incompleto",
            Email = "mod@checkpoint.com",
            Senha = "SenhaSegura123!",
            Codigo = "999888",
            TipoPerfil = "Moderador",
            CodigoCredencial = null,
            SetorModeracao = "Geral"
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CadastrarAsync(request));
        Assert.Equal("O código de credencial é obrigatório para moderadores.", ex.Message);
    }

    [Fact]
    public async Task CadastrarAsync_FotoInsegura_LancaValidationException()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService { FotoSegura = false };
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["jogador@checkpoint.com"] = "123456";

        var request = new CadastroRequestDto
        {
            Nome = "Jogador Improprio",
            Email = "jogador@checkpoint.com",
            Senha = "SenhaSegura123!",
            Codigo = "123456",
            FotoPerfil = "https://imagem-inapropriada.com/nsfw.jpg"
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CadastrarAsync(request));
        Assert.Equal("A foto de perfil contém conteúdo impróprio ou ofensivo.", ex.Message);
    }

    [Fact]
    public async Task CadastrarAsync_CodigoIncorreto_LancaValidationException()
    {
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var fakeModeracao = new FakeModeracaoFotoService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail, fakeModeracao);

        fakeCodigo.Codigos["john@beechershope.com"] = "654321";

        var request = new CadastroRequestDto
        {
            Nome = "John Marston",
            Email = "john@beechershope.com",
            Senha = "SenhaSegura123!",
            Codigo = "000000"
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CadastrarAsync(request));
        Assert.Equal("Código de verificação inválido ou expirado.", ex.Message);
    }
}
