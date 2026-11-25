namespace Domain.Model;

public abstract class Catalogo
{
    public int IdCatalogo { get; set; }
    public required int IdUsuario { get; set; }
    public required List<int> Jogos { get; set; }
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
