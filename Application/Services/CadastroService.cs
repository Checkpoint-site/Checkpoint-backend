using Application.Dtos.Cadastro.Requests;
using Application.Dtos.Cadastro.Responses;
using Application.Interfaces;
using Application.Utils.Hash;
using Application.Utils.Token;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using static Application.Mappers.CadastroMapper;

namespace Application.Services;

public class CadastroService(ICadastroRepository repository) : ICadastroService
{
    public async Task<CadastroResponseDto> CadastrarAsync(CadastroRequestDto request)
    {
        var command = ToCommand(request);

        var senhaHash = Hash.GerarHashSenha(command.Senha);

        var commandComHash = command with
        {
            Email = command.Email,
            Senha = senhaHash
        };

        var usuarioCriado = await repository.CadastroRepositoryAsync(commandComHash)
            ?? throw new ConflictException("Este e-mail já está em uso.");

        string token = Token.GerarToken(
            usuarioCriado.Id,
            usuarioCriado.Email
        );

        return new CadastroResponseDto
        {
            Token = token
        };
    }
}
