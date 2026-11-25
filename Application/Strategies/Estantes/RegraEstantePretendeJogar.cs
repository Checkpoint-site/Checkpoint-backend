using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Loggers.Commands;
using Domain.Enum;

namespace Application.Strategies.Estantes;

public class RegraEstantePretendeJogar : RegraEstanteCatalogo
{
    public override TipoEstante TipoEstante => TipoEstante.PretendeJogar;
    public override string ChaveExibicao => "pretende-jogar";
    public override int LimiteMaximoJogos => 500;

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
            DeltaPretendeJogarJogo = delta,
            DeltaPretendeJogarGenero = delta
        };
    }
}
