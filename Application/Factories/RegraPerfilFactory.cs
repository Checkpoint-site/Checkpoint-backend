using Application.Interfaces;
using Application.Strategies.Perfis;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Factories;

public class RegraPerfilFactory(IEnumerable<RegraPerfilUsuario> regras) : IRegraPerfilFactory
{
    private readonly IReadOnlyDictionary<TipoPerfilUsuario, RegraPerfilUsuario> _regras =
        regras.ToDictionary(r => r.TipoPerfil);

    public RegraPerfilUsuario Criar(TipoPerfilUsuario tipoPerfil)
    {
        if (_regras.TryGetValue(tipoPerfil, out var regra))
        {
            return regra;
        }

        throw new ValidationException($"Regra de perfil '{tipoPerfil}' não está registrada.");
    }

    public RegraPerfilUsuario Criar(string? tipoPerfil)
    {
        if (string.IsNullOrWhiteSpace(tipoPerfil))
        {
            return Criar(TipoPerfilUsuario.Regular);
        }

        var normalizado = tipoPerfil.Trim().Replace("-", string.Empty).Replace("_", string.Empty);

        if (Enum.TryParse<TipoPerfilUsuario>(normalizado, ignoreCase: true, out var perfilEnum)
            && Enum.IsDefined(perfilEnum))
        {
            return Criar(perfilEnum);
        }

        var disponiveis = string.Join(", ", _regras.Values.Select(r => r.ChaveExibicao));
        throw new ValidationException($"Tipo de perfil '{tipoPerfil}' inválido. Valores aceitos: {disponiveis}.");
    }
}
