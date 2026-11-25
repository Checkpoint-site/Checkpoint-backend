using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Estantes.Respost;
using Domain.Dtos.Loggers.Commands;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Estantes;

public class RegraEstanteFavoritos : RegraEstanteCatalogo
{
    public override TipoEstante TipoEstante => TipoEstante.Favoritos;
    public override string ChaveExibicao => "favoritos";
    public override int LimiteMaximoJogos => 100;

    public override void ValidarAdicaoJogo(EstanteCatalogoRespostDto estante, JogoCatalogoRespostDto jogo)
    {
        base.ValidarAdicaoJogo(estante, jogo);

        if (jogo.EmBreve)
        {
            throw new ValidationException("Jogos ainda não lançados ('Em Breve') não podem ser adicionados aos Favoritos; utilize a estante 'Pretende Jogar'.");
        }
    }

    public override ImpactoEngajamentoEstanteCommandDto CriarComandoImpactoLoggers(
        int idUsuario,
        JogoCatalogoRespostDto jogo,
        bool adicionando)
    {
        var delta = ObterDelta(adicionando);

        return new ImpactoEngajamentoEstanteCommandDto
        {
            IdJogo = jogo.IdJogo,
            IdUsuario = idUsuario,
            Generos = jogo.Generos.Distinct().ToList(),
            DeltaFavoritosJogo = delta,
            DeltaFavoritosGenero = delta,
            DeltaFavoritosUsuario = delta
        };
    }
}
