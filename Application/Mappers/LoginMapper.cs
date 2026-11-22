using Application.Dtos.Login.Requests;
using Domain.Dtos.Login.Commands;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class LoginMapper
{
    public static partial LoginCommandsDto ToCommand(LoginRequestDto Request);

}
