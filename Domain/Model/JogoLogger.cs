namespace Domain.Model;

public class JogoLogger : EngajamentoLogger
{
    public required int IdJogo { get; set; }
    public ulong PresencaPretendeJogar { get; set; }
    public double SomaFogueiras { get; set; }
    public ulong TotalAvaliacoes { get; set; }
    public double MediaFogueiras { get; set; }
}
