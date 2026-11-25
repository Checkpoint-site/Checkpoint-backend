using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Strategies.Estantes;

public class RegraEstanteFavoritos : RegraEstanteCatalogo
{
    public override string TipoEstante => "favoritos";
    public override int LimiteMaximoJogos => 100;

    public override void ValidarAdicaoJogo(Catalogo estante, Jogo jogo)
    {
        base.ValidarAdicaoJogo(estante, jogo);

        if (jogo.EmBreve)
        {
            throw new ValidationException("Jogos ainda não lançados ('Em Breve') não podem ser adicionados aos Favoritos; utilize a estante 'Pretende Jogar'.");
        }
    }

    public override void AtualizarLoggers(
        JogoLogger jogoLogger,
        IEnumerable<GeneroLogger> generoLoggers,
        UsuarioLogger usuarioLogger,
        bool adicionando)
    {
        var agora = DateTime.UtcNow;

        jogoLogger.PresencaFavoritos = AjustarContador(jogoLogger.PresencaFavoritos, adicionando);
        jogoLogger.UltimaAtualizacao = agora;

        usuarioLogger.PresencaFavoritos = AjustarContador(usuarioLogger.PresencaFavoritos, adicionando);
        usuarioLogger.UltimaAtualizacao = agora;

        foreach (var generoLogger in generoLoggers)
        {
            generoLogger.PresencaFavoritos = AjustarContador(generoLogger.PresencaFavoritos, adicionando);
            generoLogger.UltimaAtualizacao = agora;
        }
    }
}
