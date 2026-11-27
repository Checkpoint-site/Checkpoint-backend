namespace Application.Dtos.Posts.Responses;

public record PostResponseDto
{
    public required int IdPost { get; init; }
    public required int IdUsuario { get; init; }
    public required string AutorNome { get; init; }
    public required string AutorHandle { get; init; }
    public required string AutorIniciais { get; init; }
    public string? AutorFotoPerfil { get; init; }
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string JogoTitulo { get; init; }
    public int JogoAno { get; init; }
    public required string JogoCapaUrl { get; init; }
    public required string DesenvolvedoraJogo { get; init; }
    public required string Titulo { get; init; }
    public required double Nota { get; init; }
    public required string Comentario { get; init; }
    public string? Status { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public int Curtidas { get; init; }
    public int TotalComentarios { get; init; }
    public bool ContemSpoiler { get; init; }
    public bool CurtidaPeloUsuario { get; init; }
    public bool DoUsuarioAtual { get; init; }
    public required string TempoAtras { get; init; }
    public required DateTime DataCriacao { get; init; }
}
