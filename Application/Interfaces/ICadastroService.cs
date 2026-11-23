using Application.Dtos.Cadastro.Requests;
using Application.Dtos.Cadastro.Responses;

namespace Application.Interfaces;

public interface ICadastroService
{
    Task<VerificarEmailResponseDto> VerificarDisponibilidadeAsync(VerificarEmailRequestDto request);
    Task<SolicitarCodigoResponseDto> SolicitarCodigoAsync(SolicitarCodigoRequestDto request);
    Task<CadastroResponseDto> CadastrarAsync(CadastroRequestDto request);
}
