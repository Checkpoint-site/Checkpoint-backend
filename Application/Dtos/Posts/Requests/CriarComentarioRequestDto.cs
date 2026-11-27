namespace Application.Dtos.Posts.Requests;

public record CriarComentarioRequestDto
{
    public required string Texto { get; init; }
    public bool ContemSpoiler { get; init; }
}
