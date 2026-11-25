namespace Domain.Model;

public class Moderador : Usuario
{
    public required string CodigoCredencial { get; set; }
    public required string SetorModeracao { get; set; }
    public int TotalAcoesModeracao { get; set; } = 0;
}
