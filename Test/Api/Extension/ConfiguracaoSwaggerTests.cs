using Api.Extension;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace Test.Api.Extension;

public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "CheckpointApi";
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

public class ConfiguracaoSwaggerTests
{
    [Fact]
    public void AddConfiguracaoSwagger_RegistraSwaggerGenESecurityDefinitionComSucesso()
    {
        // Arrange
        var services = new ServiceCollection();
        var testEnv = new TestWebHostEnvironment();
        services.AddLogging();
        services.AddRouting();
        services.AddSingleton<IWebHostEnvironment>(testEnv);
        services.AddSingleton<IHostEnvironment>(testEnv);
        services.AddEndpointsApiExplorer();

        // Act
        services.AddConfiguracaoSwagger();
        var provider = services.BuildServiceProvider();
        var swaggerProvider = provider.GetRequiredService<ISwaggerProvider>();
        var swaggerDoc = swaggerProvider.GetSwagger("v1");

        // Assert
        Assert.NotNull(swaggerDoc);
        Assert.Equal("Checkpoint API", swaggerDoc.Info.Title);
        Assert.Equal("v1", swaggerDoc.Info.Version);

        // Verifica o esquema de segurança (cadeado)
        Assert.NotNull(swaggerDoc.Components?.SecuritySchemes);
        Assert.True(swaggerDoc.Components.SecuritySchemes.ContainsKey("Bearer"));

        var scheme = swaggerDoc.Components.SecuritySchemes["Bearer"];
        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);

        // Verifica a exigência global de segurança
        Assert.NotNull(swaggerDoc.Security);
        Assert.NotEmpty(swaggerDoc.Security);
    }
}
