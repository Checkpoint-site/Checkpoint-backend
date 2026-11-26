using Application.Strategies.Perfis;
using Domain.Enum;

namespace Application.Interfaces;

public interface IRegraPerfilFactory
{
    RegraPerfilUsuario Criar(TipoPerfilUsuario tipoPerfil);
    RegraPerfilUsuario Criar(string? tipoPerfil);
}
