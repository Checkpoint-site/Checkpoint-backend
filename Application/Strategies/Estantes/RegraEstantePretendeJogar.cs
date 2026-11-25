using Domain.Model;

namespace Application.Strategies.Estantes;

public class RegraEstantePretendeJogar : RegraEstanteCatalogo
{
    public override string TipoEstante => "pretende-jogar";
    public override int LimiteMaximoJogos => 500;

    public override void AtualizarLoggers(
        JogoLogger jogoLogger,
        IEnumerable<GeneroLogger> generoLoggers,
        UsuarioLogger usuarioLogger,
        bool adicionando)
    {
        var agora = DateTime.UtcNow;

        jogoLogger.PresencaPretendeJogar = AjustarContador(jogoLogger.PresencaPretendeJogar, adicionando);
        jogoLogger.UltimaAtualizacao = agora;

        usuarioLogger.UltimaAtualizacao = agora;

        foreach (var generoLogger in generoLoggers)
        {
            generoLogger.PresencaPretendeJogar = AjustarContador(generoLogger.PresencaPretendeJogar, adicionando);
            generoLogger.UltimaAtualizacao = agora;
        }
    }
}
