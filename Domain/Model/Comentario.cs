namespace Domain.Model;

public class Comentario
{
    public int IdComentario { get; set; }
    public required int IdPost { get; set; }
    public required int IdUsuario { get; set; }
    public required string Conteudo { get; set; }
    public int QuantidadeCurtidas { get; set; }
    public required DateTime DataCriacao { get; set; }
}
