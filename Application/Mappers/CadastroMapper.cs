using Application.Dtos.Cadastro.Requests;
using Domain.Dtos.Cadastro.Commands;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CadastroMapper
{
    [MapperIgnoreSource(nameof(CadastroRequestDto.Codigo))]
    public static partial CadastroCommandsDto ToCommand(CadastroRequestDto request);
}
