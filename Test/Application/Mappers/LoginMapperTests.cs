using Application.Dtos.Login.Requests;
using Application.Mappers;
using Domain.Dtos.Login.Commands;
using Xunit;

namespace Test.Application.Mappers;

public class LoginMapperTests
{
    [Fact]
    public void ToCommand_ComRequestValido_MapeiaParaLoginCommandsDto()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "geralt@kaermorhen.com",
            Senha = "SenhaForte123!"
        };

        // Act
        var command = LoginMapper.ToCommand(request);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(request.Email, command.Email);
        Assert.Equal(request.Senha, command.Senha);
    }
}
