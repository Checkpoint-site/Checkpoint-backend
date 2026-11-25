namespace Domain.Model;

public class Favoritos : Catalogo
{
    public int? JogoDestaquePerfilId { get; set; }
    public bool ExibirNoTopoPerfil { get; set; } = true;
    public string? ObservacaoDestaque { get; set; }
}
