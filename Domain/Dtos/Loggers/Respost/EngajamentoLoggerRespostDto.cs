namespace Domain.Dtos.Loggers.Respost;

public abstract record EngajamentoLoggerRespostDto
{
    public int IdLogger { get; init; }
    public ulong QuantidadeCurtida { get; init; }
    public ulong PresencaPost { get; init; }
    public ulong PresencaComentario { get; init; }
    public ulong PresencaFavoritos { get; init; }
    public DateTime UltimaAtualizacao { get; init; }
}
