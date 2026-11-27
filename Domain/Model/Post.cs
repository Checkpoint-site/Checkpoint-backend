using Domain.Enum;

namespace Domain.Model;

public class Post
{
    public int IdPost { get; set; }
    public required int IdUsuario { get; set; }
    public required int IdJogo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public required double Fogueiras { get; set; }
    public required string Conteudo { get; set; }
    public required List<Generos> TipoPost { get; set; }
    public StatusJornada? StatusJornada { get; set; }
    public List<string> Tags { get; set; } = [];
    public int QuantidadeCurtidas { get; set; }
    public int QuantidadeComentarios { get; set; }
    public bool TemSpoiler { get; set; } = false;
    public bool PostDeletado { get; set; } = false;
    public required DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
