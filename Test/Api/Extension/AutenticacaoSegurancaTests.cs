using Api.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Test.Api.Extension;

[Collection("Sequential")]
public class AutenticacaoSegurancaTests : IDisposable
{
    public AutenticacaoSegurancaTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        Environment.SetEnvironmentVariable("Token__Secret", null);
        Environment.SetEnvironmentVariable("TOKEN_SECRET", null);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        Environment.SetEnvironmentVariable("Token__Secret", null);
        Environment.SetEnvironmentVariable("TOKEN_SECRET", null);
    }

    [Fact]
    public void AddAutenticacaoSeguranca_ComChaveValida_RegistraServicosComSucesso()
    {
        // Arrange
        var services = new ServiceCollection();
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "ChaveSecretaValidaComPeloMenos32Caracteres123!");
        var config = new ConfigurationBuilder().Build();

        // Act
        services.AddAutenticacaoSeguranca(config);
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddAutenticacaoSeguranca_ChaveMenorQue32Caracteres_LancaInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "ChaveCurta123");
        var config = new ConfigurationBuilder().Build();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => services.AddAutenticacaoSeguranca(config));
        Assert.Equal("A chave secreta do JWT deve ter pelo menos 32 caracteres.", ex.Message);
    }

    [Fact]
    public void AddAutenticacaoSeguranca_SemChaveConfigurada_LancaInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => services.AddAutenticacaoSeguranca(config));
        Assert.Equal("A chave secreta do JWT não foi configurada.", ex.Message);
    }
}
