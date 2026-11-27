namespace Application.Dtos.Posts.Responses;

public record PostComComentariosResponseDto
{
    public required PostResponseDto Review { get; init; }
    public required IReadOnlyList<ComentarioResponseDto> Comentarios { get; init; }
    public required IReadOnlyList<PostResponseDto> OutrasReviews { get; init; }
}
