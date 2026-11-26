using Application.Dtos.Cadastro.Requests;
using Domain.Dtos.Perfis.Commands;
using Domain.Dtos.Perfis.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Perfis;

public class RegraPerfilRegular : RegraPerfilUsuario
{
    public override TipoPerfilUsuario TipoPerfil => TipoPerfilUsuario.Regular;
    public override string ChaveExibicao => "regular";
    public override string RoleJwt => "Regular";
    public override int LimiteInicialListas => 15;
    public override bool PodeModerarConteudo => false;

    public override void ValidarDadosCadastro(CadastroRequestDto request)
    {
    }

    public AtualizarExperienciaCommandDto CriarComandoAdicionarExperiencia(
        PerfilRegularRespostDto usuario,
        int pontos)
    {
        if (pontos <= 0)
        {
            throw new ValidationException("A pontuação de experiência deve ser positiva.");
        }

        int nivelAtual = usuario.NivelGamer;
        int experienciaAtual = usuario.PontosExperiencia + pontos;
        int limiteListasAtual = usuario.LimiteListas;

        int pontosParaProximoNivel = nivelAtual * 100;
        while (experienciaAtual >= pontosParaProximoNivel)
        {
            nivelAtual++;
            experienciaAtual -= pontosParaProximoNivel;
            limiteListasAtual += 5;
            pontosParaProximoNivel = nivelAtual * 100;
        }

        return new AtualizarExperienciaCommandDto
        {
            IdUsuario = usuario.Id,
            NivelGamer = nivelAtual,
            PontosExperiencia = experienciaAtual,
            LimiteListas = limiteListasAtual
        };
    }
}
