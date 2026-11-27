namespace Application.Dtos.Diario.Responses;

public record RegistroJogoUsuarioResponseDto
{
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string JogoTitulo { get; init; }
    public required string JogoCapaUrl { get; init; }
    public int JogoAno { get; init; }
    public required string Desenvolvedora { get; init; }
    public IReadOnlyList<string> Generos { get; init; } = [];
    public double NotaMediaJogo { get; init; }
    public int TotalAvaliacoesJogo { get; init; }
    public string? Status { get; init; }
    public bool Jogado { get; init; }
    public bool QueroJogar { get; init; }
    public bool Favorito { get; init; }
    public double? Nota { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public int? IdPostReview { get; init; }
    public string? Comentario { get; init; }
    public bool ContemSpoiler { get; init; }
    public required DateTime AtualizadoEm { get; init; }
}
