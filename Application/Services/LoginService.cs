using Application.Dtos.Login.Requests;
using Application.Dtos.Login.Responses;
using Application.Interfaces;
using Application.Utils.Hash;
using Application.Utils.Token;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using static Application.Mappers.LoginMapper;

namespace Application.Services;

public class LoginService(ILoginRepository repository) : ILoginService
{
    public async Task<LoginResponseDto> AutenticarAsync(LoginRequestDto request)
    {
        var command = ToCommand(request);

        var usuarioDto = await repository.LoginRepositoryAsync(command)
            ?? throw new AuthenticationException("E-mail ou senha inválidos.");

        if (!Hash.VerificarSenha(command.Senha, usuarioDto.Senha))
        {
            throw new AuthenticationException("E-mail ou senha inválidos.");
        }

        if (usuarioDto.SuspensoAte.HasValue && usuarioDto.SuspensoAte.Value > DateTime.UtcNow)
        {
            throw new ForbiddenException("Esta conta está temporariamente suspensa.");
        }

        string token = Token.GerarToken(
            usuarioDto.Id,
            usuarioDto.Email,
            usuarioDto.TipoPerfil.ToString(),
            nome: usuarioDto.Nome
        );

        return new LoginResponseDto
        {
            Token = token
        };
    }
}
