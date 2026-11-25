using Application.Dtos.Cadastro.Requests;
using Domain.Model;

namespace Application.Regras.Perfis;

public abstract class RegraPerfilUsuario
{
    public abstract string ObterRole();
    public abstract int ObterLimiteInicialListas();
    public abstract bool PodeModerarConteudo();

    public virtual bool EstaSuspenso(Usuario usuario) =>
        usuario.SuspensoAte.HasValue && usuario.SuspensoAte.Value > DateTime.UtcNow;

    public virtual bool PodeCriarLista(Usuario usuario) =>
        !usuario.ContaDeletada && !EstaSuspenso(usuario);

    public abstract void ValidarDadosCadastro(CadastroRequestDto request);

    public static RegraPerfilUsuario Criar(string? tipoPerfil)
    {
        if (string.Equals(tipoPerfil, "Moderador", StringComparison.OrdinalIgnoreCase))
        {
            return new RegraPerfilModerador();
        }

        return new RegraPerfilRegular();
    }
}
