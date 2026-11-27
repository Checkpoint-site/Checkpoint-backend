namespace Domain.Dtos.Loggers.Respost;

public record UsuarioLoggerRespostDto : EngajamentoLoggerRespostDto
{
    public required int IdUsuario { get; init; }
    public ulong CurtidasRecebidas { get; init; }
    public ulong QuantidadeSeguidores { get; init; }
    public bool PossuiFotoPerfil { get; init; }
    public double SomaFogueirasDadas { get; init; }
    public ulong TotalAvaliacoes { get; init; }
    public double MediaFogueiras { get; init; }
}
