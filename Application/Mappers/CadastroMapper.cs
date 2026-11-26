using Application.Dtos.Cadastro.Requests;
using Domain.Dtos.Cadastro.Commands;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper]
public static partial class CadastroMapper
{
    [MapperIgnoreSource(nameof(CadastroRequestDto.Codigo))]
    [MapperIgnoreSource(nameof(CadastroRequestDto.TipoPerfil))]
    [MapperIgnoreTarget(nameof(CadastroCommandsDto.TipoPerfil))]
    [MapperIgnoreTarget(nameof(CadastroCommandsDto.LimiteListasInicial))]
    public static partial CadastroCommandsDto ToCommand(CadastroRequestDto request);
}
