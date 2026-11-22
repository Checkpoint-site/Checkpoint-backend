using Application.Dtos.Cadastro.Requests;
using Application.Services;
using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Response;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
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

    [Fact]
    public async Task CadastrarAsync_DadosValidos_CadastraComSucessoERetornaToken()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var service = new CadastroService(fakeRepo);

        var request = new CadastroRequestDto
        {
            Nome = "Arthur Morgan",
            Email = "arthur@van-der-linde.com",
            Senha = "SenhaSegura123!"
        };

        // Act
        var resultado = await service.CadastrarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
    }

    [Fact]
    public async Task CadastrarAsync_EmailJaExistente_LancaConflictException()
    {
        // Arrange
        var fakeRepo = new FakeCadastroRepository();
        var service = new CadastroService(fakeRepo);

        var requestInicial = new CadastroRequestDto
        {
            Nome = "John Marston",
            Email = "john@beechershope.com",
            Senha = "SenhaOriginal123!"
        };
        await service.CadastrarAsync(requestInicial);

        var requestDuplicado = new CadastroRequestDto
        {
            Nome = "Outro John",
            Email = "JOHN@beechershope.com",
            Senha = "OutraSenha123!"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.CadastrarAsync(requestDuplicado));
        Assert.Equal("Este e-mail já está em uso.", ex.Message);
    }
}
