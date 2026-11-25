namespace Domain.Dtos.Loggers.Respost;

public record JogoLoggerRespostDto : EngajamentoLoggerRespostDto
{
    public required int IdJogo { get; init; }
    public ulong PresencaPretendeJogar { get; init; }
    public ulong SomaFogueiras { get; init; }
}
