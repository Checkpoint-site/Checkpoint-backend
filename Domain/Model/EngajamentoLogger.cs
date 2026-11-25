namespace Domain.Model;

public abstract class EngajamentoLogger
{
    public int IdLogger { get; set; }
    public ulong QuantidadeCurtida { get; set; }
    public ulong PresencaPost { get; set; }
    public ulong PresencaComentario { get; set; }
    public ulong PresencaFavoritos { get; set; }
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
}
