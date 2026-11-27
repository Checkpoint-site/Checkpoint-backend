using Domain.Enum;

namespace Domain.Dtos.Diario.Respost;

public record RegistroDiarioRespostDto
{
    public required int IdRegistroDiario { get; init; }
    public required int IdUsuario { get; init; }
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string NomeJogo { get; init; }
    public required string ImagemJogo { get; init; }
    public int? AnoLancamentoJogo { get; init; }
    public required string DesenvolvedoraJogo { get; init; }
    public IReadOnlyList<Generos> GenerosJogo { get; init; } = [];
    public double MediaFogueirasJogo { get; init; }
    public int TotalAvaliacoesJogo { get; init; }
    public bool Jogado { get; init; }
    public bool QueroJogar { get; init; }
    public bool Favorito { get; init; }
    public StatusJornada? Status { get; init; }
    public double? NotaFogueiras { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public int? IdPostReview { get; init; }
    public string? ComentarioReview { get; init; }
    public bool ContemSpoilerReview { get; init; }
    public required DateTime DataAtualizacao { get; init; }
}
