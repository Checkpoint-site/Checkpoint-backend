using Application.Dtos.Cadastro.Requests;
using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Regras.Perfis;

public class RegraPerfilModerador : RegraPerfilUsuario
{
    public override string ObterRole() => "Moderador";

    public override int ObterLimiteInicialListas() => 100;

    public override bool PodeModerarConteudo() => true;

    public override void ValidarDadosCadastro(CadastroRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoCredencial))
        {
            throw new ValidationException("O código de credencial é obrigatório para moderadores.");
        }

        if (string.IsNullOrWhiteSpace(request.SetorModeracao))
        {
            throw new ValidationException("O setor de moderação é obrigatório para moderadores.");
        }
    }

    public void SuspenderUsuario(Moderador moderador, Usuario usuarioAlvo, int diasSuspensao, string motivo)
    {
        if (usuarioAlvo.Id != 0 && usuarioAlvo.Id == moderador.Id)
        {
            throw new ValidationException("Um moderador não pode suspender a própria conta.");
        }

        if (diasSuspensao <= 0)
        {
            throw new ValidationException("O período de suspensão deve ser de no mínimo 1 dia.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ValidationException("O motivo da suspensão é obrigatório.");
        }

        usuarioAlvo.SuspensoAte = DateTime.UtcNow.AddDays(diasSuspensao);
        moderador.TotalAcoesModeracao++;
    }

    public void ReativarUsuario(Moderador moderador, Usuario usuarioAlvo)
    {
        usuarioAlvo.SuspensoAte = null;
        moderador.TotalAcoesModeracao++;
    }
}
