using Application.Dtos.Cadastro.Requests;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Regras.Perfis;

public class RegraPerfilRegular : RegraPerfilUsuario
{
    public override string ObterRole() => "Regular";

    public override int ObterLimiteInicialListas() => 15;

    public override bool PodeModerarConteudo() => false;

    public override void ValidarDadosCadastro(CadastroRequestDto request)
    {
    }

    public void AdicionarExperiencia(UsuarioRegular usuario, int pontos)
    {
        if (pontos <= 0)
        {
            throw new ValidationException("A pontuação de experiência deve ser positiva.");
        }

        usuario.PontosExperiencia += pontos;

        int pontosParaProximoNivel = usuario.NivelGamer * 100;
        while (usuario.PontosExperiencia >= pontosParaProximoNivel)
        {
            usuario.NivelGamer++;
            usuario.PontosExperiencia -= pontosParaProximoNivel;
            usuario.LimiteListas += 5;
            pontosParaProximoNivel = usuario.NivelGamer * 100;
        }
    }
}
