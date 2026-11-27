using Domain.Enum;

namespace Domain.Dtos.Posts.Commands;

public record SalvarPostCommandDto
{
    public required int IdUsuario { get; init; }
    public required int IdJogo { get; init; }
    public string? Titulo { get; init; }
    public required double Fogueiras { get; init; }
    public required string Conteudo { get; init; }
    public required IReadOnlyList<Generos> GenerosJogo { get; init; }
    public StatusJornada? StatusJornada { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public bool TemSpoiler { get; init; }
}
