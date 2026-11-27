namespace Domain.Dtos.Posts.Respost;

public record ComentarioRespostDto
{
    public required int IdComentario { get; init; }
    public required int IdPost { get; init; }
    public required int IdUsuario { get; init; }
    public required string NomeAutor { get; init; }
    public string? FotoPerfilAutor { get; init; }
    public required string Conteudo { get; init; }
    public int QuantidadeCurtidas { get; init; }
    public bool TemSpoiler { get; init; }
    public bool CurtidoPeloUsuarioAtual { get; init; }
    public bool DoUsuarioAtual { get; init; }
    public required DateTime DataCriacao { get; init; }
}
