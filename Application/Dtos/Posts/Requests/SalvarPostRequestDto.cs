namespace Application.Dtos.Posts.Requests;

public record SalvarPostRequestDto
{
    public string? Titulo { get; init; }
    public required double Nota { get; init; }
    public required string Comentario { get; init; }
    public string? Status { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
    public bool ContemSpoiler { get; init; }
}
