using Application.Dtos.Login.Requests;
using Application.Factories;
using Application.Interfaces;
using Application.Services;
using Application.Strategies.Perfis;
using Application.Utils.Hash;
using Domain.Dtos.Login.Commands;
using Domain.Dtos.Login.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;

namespace Test.Application.Services;

[Collection("Sequential")]
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

    private static IRegraPerfilFactory CriarFactoryPerfis() =>
        new RegraPerfilFactory([new RegraPerfilRegular(), new RegraPerfilModerador()]);

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
        var fakeRepo = new FakeLoginRepository();
        var senhaPura = "SenhaForte123!";
        var senhaHash = Hash.GerarHashSenha(senhaPura);

        var usuario = new LoginRespostDto
        {
            Id = 1,
            Email = "cloud@midgar.com",
            Senha = senhaHash,
            TipoPerfil = TipoPerfilUsuario.Regular
        };
        fakeRepo.Adicionar(usuario);

        var service = new LoginService(fakeRepo, CriarFactoryPerfis());

        var request = new LoginRequestDto
        {
            Email = "cloud@midgar.com",
            Senha = senhaPura
        };

        var resultado = await service.AutenticarAsync(request);

        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
    }

    [Fact]
    public async Task AutenticarAsync_UsuarioSuspenso_LancaForbiddenException()
    {
        var fakeRepo = new FakeLoginRepository();
        var senhaPura = "SenhaForte123!";
        var senhaHash = Hash.GerarHashSenha(senhaPura);

        var usuario = new LoginRespostDto
        {
            Id = 2,
            Email = "suspenso@checkpoint.com",
            Senha = senhaHash,
            TipoPerfil = TipoPerfilUsuario.Regular,
            SuspensoAte = DateTime.UtcNow.AddDays(5)
        };
        fakeRepo.Adicionar(usuario);

        var service = new LoginService(fakeRepo, CriarFactoryPerfis());

        var request = new LoginRequestDto
        {
            Email = "suspenso@checkpoint.com",
            Senha = senhaPura
        };

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => service.AutenticarAsync(request));
        Assert.Equal("Esta conta está temporariamente suspensa.", ex.Message);
    }

    [Fact]
    public async Task AutenticarAsync_EmailInexistente_LancaAuthenticationException()
    {
        var fakeRepo = new FakeLoginRepository();
        var service = new LoginService(fakeRepo, CriarFactoryPerfis());

        var request = new LoginRequestDto
        {
            Email = "fantasma@midgar.com",
            Senha = "QualquerSenha123!"
        };

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => service.AutenticarAsync(request));
        Assert.Equal("E-mail ou senha inválidos.", ex.Message);
    }

    [Fact]
    public async Task AutenticarAsync_SenhaIncorreta_LancaAuthenticationException()
    {
        var fakeRepo = new FakeLoginRepository();
        var senhaCorretaHash = Hash.GerarHashSenha("SenhaCorreta123!");

        var usuario = new LoginRespostDto
        {
            Id = 1,
            Email = "tifa@midgar.com",
            Senha = senhaCorretaHash,
            TipoPerfil = TipoPerfilUsuario.Regular
        };
        fakeRepo.Adicionar(usuario);

        var service = new LoginService(fakeRepo, CriarFactoryPerfis());

        var request = new LoginRequestDto
        {
            Email = "tifa@midgar.com",
            Senha = "SenhaErrada123!"
        };

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => service.AutenticarAsync(request));
        Assert.Equal("E-mail ou senha inválidos.", ex.Message);
    }
}
