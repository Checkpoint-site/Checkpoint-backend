using Application.Dtos.Cadastro.Requests;
using Application.Mappers;
using Domain.Dtos.Cadastro.Commands;
using Xunit;

namespace Test.Application.Mappers;

public class CadastroMapperTests
{
    [Fact]
    public void ToCommand_ComRequestValido_MapeiaParaCadastroCommandsDto()
    {
        // Arrange
        var request = new CadastroRequestDto
        {
            Nome = "Geralt de Rivia",
            Email = "geralt@kaermorhen.com",
            Senha = "SenhaForte123!"
        };

        // Act
        var command = CadastroMapper.ToCommand(request);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(request.Nome, command.Nome);
        Assert.Equal(request.Email, command.Email);
        Assert.Equal(request.Senha, command.Senha);
    }
}
