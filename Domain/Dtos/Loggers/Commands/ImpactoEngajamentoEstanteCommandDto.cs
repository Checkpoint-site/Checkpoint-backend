using Domain.Enum;

namespace Domain.Dtos.Loggers.Commands;

public record ImpactoEngajamentoEstanteCommandDto
{
    public required int IdJogo { get; init; }
    public required int IdUsuario { get; init; }
    public required IReadOnlyList<Generos> Generos { get; init; }
    public int DeltaFavoritosJogo { get; init; }
    public int DeltaPretendeJogarJogo { get; init; }
    public int DeltaFavoritosGenero { get; init; }
    public int DeltaPretendeJogarGenero { get; init; }
    public int DeltaFavoritosUsuario { get; init; }
}
