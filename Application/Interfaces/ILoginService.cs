using Application.Dtos.Login.Requests;
using Application.Dtos.Login.Responses;

namespace Application.Interfaces;

public interface ILoginService
{
    Task<LoginResponseDto> AutenticarAsync(LoginRequestDto request);
}
