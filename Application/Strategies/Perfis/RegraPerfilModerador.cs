using Application.Dtos.Cadastro.Requests;
using Domain.Dtos.Perfis.Commands;
using Domain.Dtos.Perfis.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Perfis;

public class RegraPerfilModerador : RegraPerfilUsuario
{
    public override TipoPerfilUsuario TipoPerfil => TipoPerfilUsuario.Moderador;
    public override string ChaveExibicao => "moderador";
    public override string RoleJwt => "Moderador";
    public override int LimiteInicialListas => 100;
    public override bool PodeModerarConteudo => true;

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

    public AlterarSuspensaoUsuarioCommandDto CriarComandoSuspensao(
        PerfilModeradorRespostDto moderador,
        PerfilUsuarioRespostDto usuarioAlvo,
        int diasSuspensao,
        string motivo)
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

        return new AlterarSuspensaoUsuarioCommandDto
        {
            IdModerador = moderador.Id,
            IdUsuarioAlvo = usuarioAlvo.Id,
            SuspensoAte = DateTime.UtcNow.AddDays(diasSuspensao),
            Motivo = motivo.Trim(),
            NovoTotalAcoesModeracao = moderador.TotalAcoesModeracao + 1
        };
    }

    public AlterarSuspensaoUsuarioCommandDto CriarComandoReativacao(
        PerfilModeradorRespostDto moderador,
        PerfilUsuarioRespostDto usuarioAlvo)
    {
        return new AlterarSuspensaoUsuarioCommandDto
        {
            IdModerador = moderador.Id,
            IdUsuarioAlvo = usuarioAlvo.Id,
            SuspensoAte = null,
            Motivo = null,
            NovoTotalAcoesModeracao = moderador.TotalAcoesModeracao + 1
        };
    }
}
