namespace Domain.Dtos.Loggers.Respost;

public record JogoLoggerRespostDto : EngajamentoLoggerRespostDto
{
    public required int IdJogo { get; init; }
    public ulong PresencaPretendeJogar { get; init; }
    public double SomaFogueiras { get; init; }
    public ulong TotalAvaliacoes { get; init; }
    public double MediaFogueiras { get; init; }
}
