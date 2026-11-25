using Domain.Enum;

namespace Domain.Model;

public class GeneroLogger : EngajamentoLogger
{
    public required Generos Genero { get; set; }
    public ulong PresencaPretendeJogar { get; set; }
}
