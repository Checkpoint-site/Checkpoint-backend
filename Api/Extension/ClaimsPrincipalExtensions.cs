using System.Security.Claims;
using Domain.Exceptions.Common;

namespace Api.Extension;

public static class ClaimsPrincipalExtensions
{
    public static int? ObterIdUsuarioOpcional(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return int.TryParse(valor, out var idUsuario) && idUsuario > 0
            ? idUsuario
            : null;
    }

    public static int ObterIdUsuarioObrigatorio(this ClaimsPrincipal user)
    {
        return user.ObterIdUsuarioOpcional()
            ?? throw new AuthenticationException("Não foi possível identificar o usuário autenticado no token.");
    }

    public static string? ObterRoleOpcional(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirstValue(ClaimTypes.Role)
            ?? user.FindFirstValue("role");
    }
}
