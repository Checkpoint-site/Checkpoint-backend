using Application.Dtos.Login.Requests;
using Application.Services;
using Application.Utils.Hash;
using Domain.Dtos.Login.Commands;
using Domain.Dtos.Login.Response;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Xunit;

namespace Test.Application.Services;

public class LoginServiceTests : IDisposable
{
    private const string ChaveSecretaTeste = "ChaveSecretaSuperSeguraParaTestesComMaisDe32Caracteres!";

    public LoginServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", ChaveSecretaTeste);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
    }

    private sealed class FakeLoginRepository : ILoginRepository
    {
        private readonly List<LoginRespostDto> _usuarios = [];

        public void Adicionar(LoginRespostDto usuario) => _usuarios.Add(usuario);

        public Task<LoginRespostDto?> LoginRepositoryAsync(LoginCommandsDto command)
        {
            var usuario = _usuarios.FirstOrDefault(u =>
                u.Email.Equals(command.Email, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(usuario);
        }
    }

    [Fact]
    public async Task AutenticarAsync_CredenciaisValidas_RetornaLoginResponseDtoComToken()
    {
        // Arrange
        var fakeRepo = new FakeLoginRepository();
        var senhaPura = "SenhaForte123!";
        var senhaHash = Hash.GerarHashSenha(senhaPura);

        var usuario = new LoginRespostDto
        {
            Id = 1,
            Email = "cloud@midgar.com",
            Senha = senhaHash
        };
        fakeRepo.Adicionar(usuario);

        var service = new LoginService(fakeRepo);

        var request = new LoginRequestDto
        {
            Email = "cloud@midgar.com",
            Senha = senhaPura
        };

        // Act
        var resultado = await service.AutenticarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
    }

    [Fact]
    public async Task AutenticarAsync_EmailInexistente_LancaAuthenticationException()
    {
        // Arrange
        var fakeRepo = new FakeLoginRepository();
        var service = new LoginService(fakeRepo);

        var request = new LoginRequestDto
        {
            Email = "inexistente@midgar.com",
            Senha = "SenhaQualquer123!"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => service.AutenticarAsync(request));
        Assert.Equal("E-mail ou senha inválidos.", ex.Message);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaIncorreta_LancaAuthenticationException()
    {
        // Arrange
        var fakeRepo = new FakeLoginRepository();
        var service = new LoginService(fakeRepo);

        var request = new LoginRequestDto
        {
            Email = "tifa@seventhheaven.com",
            Senha = "SenhaErrada123!"
        };

        var usuario = new LoginRespostDto
        {
            Id = 2,
            Email = "tifa@seventhheaven.com",
            Senha = Hash.GerarHashSenha("SenhaCorreta123!")
        };
        fakeRepo.Adicionar(usuario);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => service.AutenticarAsync(request));
        Assert.Equal("E-mail ou senha inválidos.", ex.Message);
    }
}
