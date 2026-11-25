namespace Domain.Model;

public class JogoLogger : EngajamentoLogger
{
    public required int IdJogo { get; set; }
    public ulong PresencaPretendeJogar { get; set; }
    public ulong SomaFogueiras { get; set; }
}
