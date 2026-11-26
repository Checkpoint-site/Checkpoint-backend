using Application.Dtos.Cadastro.Requests;
using Domain.Dtos.Perfis.Respost;
using Domain.Enum;

namespace Application.Strategies.Perfis;

public abstract class RegraPerfilUsuario
{
    public abstract TipoPerfilUsuario TipoPerfil { get; }
    public abstract string ChaveExibicao { get; }
    public abstract string RoleJwt { get; }
    public abstract int LimiteInicialListas { get; }
    public abstract bool PodeModerarConteudo { get; }

    public virtual bool EstaSuspenso(DateTime? suspensoAte) =>
        suspensoAte.HasValue && suspensoAte.Value > DateTime.UtcNow;

    public virtual bool EstaSuspenso(PerfilUsuarioRespostDto usuario) =>
        EstaSuspenso(usuario.SuspensoAte);

    public virtual bool PodeCriarLista(PerfilUsuarioRespostDto usuario) =>
        !usuario.ContaDeletada && !EstaSuspenso(usuario);

    public abstract void ValidarDadosCadastro(CadastroRequestDto request);
}
