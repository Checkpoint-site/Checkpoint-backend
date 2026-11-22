using Application.Dtos.Login.Requests;
using Application.Dtos.Login.Responses;
using Application.Interfaces;
using Application.Utils.Hash;
using Application.Utils.Token;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using ValidationException = Domain.Exceptions.Common.ValidationException;

using static Application.Mappers.LoginMapper;

namespace Application.Services;

public class LoginService(ILoginRepository Repository) : ILoginService
{

    public async Task<LoginResponseDto> AutenticarAsync(LoginRequestDto request)
    {
        var command = ToCommand(request);

        var usuarioDto = await Repository.LoginRepositoryAsync(command)
        ?? throw new AuthenticationException("E-mail ou senha inválidos.");

        if (!Hash.VerificarSenha(command.Senha, usuarioDto.Senha))
        {
            throw new AuthenticationException("E-mail ou senha inválidos.");
        }

        string token = Token.GerarToken(
            usuarioDto.Id,
            usuarioDto.Email
        );


        return new LoginResponseDto
        {
            Token = token
        };
    }

}
