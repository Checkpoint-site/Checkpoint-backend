namespace Application.Dtos.Posts.Responses;

public record CurtidaResponseDto
{
    public required int IdAlvo { get; init; }
    public required bool Curtido { get; init; }
    public required int QuantidadeCurtidas { get; init; }
}
