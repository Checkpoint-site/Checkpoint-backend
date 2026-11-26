namespace Domain.Model;

public class Usuario
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Senha { get; set; }
    public bool ContaDeletada { get; set; } = false;
    public DateTime? SuspensoAte { get; set; }
    public string? FotoPerfil { get; set; }
    public int QuantidadeSeguidores { get; set; } = 0;
}
