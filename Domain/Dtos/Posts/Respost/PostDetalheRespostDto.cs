using Domain.Enum;

namespace Domain.Dtos.Posts.Respost;

public record PostDetalheRespostDto
{
    public required int IdPost { get; init; }
    public required int IdUsuario { get; init; }
    public required string NomeAutor { get; init; }
    public string? FotoPerfilAutor { get; init; }
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string NomeJogo { get; init; }
    public int? AnoLancamentoJogo { get; init; }
    public required string ImagemJogo { get; init; }
    public required string DesenvolvedoraJogo { get; init; }
    public required string Titulo { get; init; }
    public required double Fogueiras { get; init; }
    public required string Conteudo { get; init; }
    public StatusJornada? StatusJornada { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public int QuantidadeCurtidas { get; init; }
    public int QuantidadeComentarios { get; init; }
    public bool TemSpoiler { get; init; }
    public bool CurtidoPeloUsuarioAtual { get; init; }
    public bool DoUsuarioAtual { get; init; }
    public required DateTime DataCriacao { get; init; }
    public DateTime? DataAtualizacao { get; init; }
}
