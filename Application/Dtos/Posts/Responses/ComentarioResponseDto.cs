namespace Application.Dtos.Posts.Responses;

public record ComentarioResponseDto
{
    public required int IdComentario { get; init; }
    public required int IdPost { get; init; }
    public required int IdUsuario { get; init; }
    public required string AutorNome { get; init; }
    public required string AutorHandle { get; init; }
    public required string AutorIniciais { get; init; }
    public string? AutorFotoPerfil { get; init; }
    public required string Texto { get; init; }
    public int Curtidas { get; init; }
    public bool ContemSpoiler { get; init; }
    public bool CurtidaPeloUsuario { get; init; }
    public bool DoUsuarioAtual { get; init; }
    public required string TempoAtras { get; init; }
    public required DateTime DataCriacao { get; init; }
}
