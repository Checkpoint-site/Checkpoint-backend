using Application.Dtos.Catalogo.Responses;
using Application.Dtos.Posts.Responses;

namespace Application.Dtos.Vitrine.Responses;

public record GeneroVitrineResponseDto
{
    public required string Slug { get; init; }
    public required string Nome { get; init; }
}

public record SecaoVitrineResponseDto
{
    public required string Id { get; init; }
    public required string Titulo { get; init; }
    public string? Subtitulo { get; init; }
    public required string Tipo { get; init; }
    public string? FiltroGenero { get; init; }
    public IReadOnlyList<JogoResumoResponseDto> Jogos { get; init; } = [];
    public IReadOnlyList<PostResponseDto> Reviews { get; init; } = [];
}

public record VitrineHomeResponseDto
{
    public required IReadOnlyList<JogoResumoResponseDto> Destaques { get; init; }
    public required IReadOnlyList<GeneroVitrineResponseDto> GenerosDisponiveis { get; init; }
    public required IReadOnlyList<SecaoVitrineResponseDto> Secoes { get; init; }
}
