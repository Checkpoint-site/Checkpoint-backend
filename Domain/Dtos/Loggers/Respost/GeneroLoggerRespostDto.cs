using Domain.Enum;

namespace Domain.Dtos.Loggers.Respost;

public record GeneroLoggerRespostDto : EngajamentoLoggerRespostDto
{
    public required Generos Genero { get; init; }
    public ulong PresencaPretendeJogar { get; init; }
}
