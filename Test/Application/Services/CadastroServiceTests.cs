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
                Email = command.Email
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
            return Task.FromResult<string?>(codigo);
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

    [Fact]
    public async Task VerificarDisponibilidadeAsync_EmailLivre_RetornaDisponivelTrue()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        var request = new VerificarEmailRequestDto
        {
            Email = "jogador_novo@checkpoint.com"
        };

        // Act
        var resultado = await service.VerificarDisponibilidadeAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.Disponivel);
    }

    [Fact]
    public async Task VerificarDisponibilidadeAsync_EmailJaCadastrado_RetornaDisponivelFalse()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        await fakeRepo.CadastroRepositoryAsync(new CadastroCommandsDto
        {
            Nome = "Veterano",
            Email = "veterano@checkpoint.com",
            Senha = "hash"
        });

        var request = new VerificarEmailRequestDto
        {
            Email = "veterano@checkpoint.com"
        };

        // Act
        var resultado = await service.VerificarDisponibilidadeAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.Disponivel);
    }

    [Fact]
    public async Task SolicitarCodigoAsync_EmailLivre_SalvaCodigoEnviaEmailERetornaSucesso()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        var request = new SolicitarCodigoRequestDto
        {
            Email = "arthur@van-der-linde.com"
        };

        // Act
        var resultado = await service.SolicitarCodigoAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Código de verificação enviado com sucesso.", resultado.Mensagem);
        Assert.True(fakeCodigo.Codigos.ContainsKey("arthur@van-der-linde.com"));
        var codigoSalvo = fakeCodigo.Codigos["arthur@van-der-linde.com"];
        Assert.Equal(6, codigoSalvo.Length);
        Assert.Single(fakeEmail.EmailsEnviados);
        Assert.Equal("arthur@van-der-linde.com", fakeEmail.EmailsEnviados[0].Destinatario);
    }

    [Fact]
    public async Task SolicitarCodigoAsync_EmailJaExistente_LancaConflictException()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        await fakeRepo.CadastroRepositoryAsync(new CadastroCommandsDto
        {
            Nome = "Arthur Morgan",
            Email = "arthur@van-der-linde.com",
            Senha = "hash"
        });

        var request = new SolicitarCodigoRequestDto
        {
            Email = "arthur@van-der-linde.com"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.SolicitarCodigoAsync(request));
        Assert.Equal("Este e-mail já está em uso.", ex.Message);
        Assert.Empty(fakeEmail.EmailsEnviados);
    }

    [Fact]
    public async Task CadastrarAsync_CodigoCorretoEDadosValidos_CadastraComSucessoERetornaToken()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        fakeCodigo.Codigos["john@beechershope.com"] = "654321";

        var request = new CadastroRequestDto
        {
            Nome = "John Marston",
            Email = "john@beechershope.com",
            Senha = "SenhaSegura123!",
            Codigo = "654321"
        };

        // Act
        var resultado = await service.CadastrarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.False(fakeCodigo.Codigos.ContainsKey("john@beechershope.com"));
    }

    [Fact]
    public async Task CadastrarAsync_CodigoIncorreto_LancaValidationException()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        fakeCodigo.Codigos["john@beechershope.com"] = "654321";

        var request = new CadastroRequestDto
        {
            Nome = "John Marston",
            Email = "john@beechershope.com",
            Senha = "SenhaSegura123!",
            Codigo = "000000"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CadastrarAsync(request));
        Assert.Equal("Código de verificação inválido ou expirado.", ex.Message);
    }

    [Fact]
    public async Task CadastrarAsync_CodigoInexistenteOuExpirado_LancaValidationException()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var fakeCodigo = new FakeCodigoVerificacaoService();
        var fakeEmail = new FakeEmailService();
        var service = new CadastroService(fakeRepo, fakeCodigo, fakeEmail);

        var request = new CadastroRequestDto
        {
            Nome = "Sadie Adler",
            Email = "sadie@bountyhunter.com",
            Senha = "SenhaSegura123!",
            Codigo = "123456"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CadastrarAsync(request));
        Assert.Equal("Código de verificação inválido ou expirado.", ex.Message);
    }
}
