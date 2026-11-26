namespace Application.Dtos.Moderacao.Responses;

public record PostSpoilerResponseDto
{
    public required int IdPost { get; init; }
    public required string Titulo { get; init; }
    public required bool TemSpoiler { get; init; }
}
