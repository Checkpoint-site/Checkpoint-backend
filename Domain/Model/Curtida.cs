namespace Domain.Model;

public abstract class Curtida
{
    public int IdCurtida { get; set; }
    public required int IdUsuario { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
