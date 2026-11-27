namespace Domain.Model;

public class UsuarioLogger : EngajamentoLogger
{
    public required int IdUsuario { get; set; }
    public ulong CurtidasRecebidas { get; set; }
    public ulong QuantidadeSeguidores { get; set; }
    public double SomaFogueirasDadas { get; set; }
    public ulong TotalAvaliacoes { get; set; }
    public double MediaFogueiras { get; set; }
}
