using Domain.Enum;

namespace Domain.Model;

public class Post
{
    public int IdPost { get; set; }
    public required int IdUsuario { get; set; }
    public required int IdJogo { get; set; }
    public required string Titulo { get; set; }
    public required int Fogueiras { get; set; }
    public required string Conteudo { get; set; }
    public required List<Generos> TipoPost { get; set; }
    public int QuantidadeCurtidas { get; set; }
    public int QuantidadeComentarios { get; set; }
    public bool PostDeletado { get; set; } = false;
    public required DateTime DataCriacao { get; set; }
}
