using Application.Dtos.Cadastro.Requests;
using Application.Dtos.Cadastro.Responses;

namespace Application.Interfaces;

public interface ICadastroService
{
    Task<CadastroResponseDto> CadastrarAsync(CadastroRequestDto request);
}
