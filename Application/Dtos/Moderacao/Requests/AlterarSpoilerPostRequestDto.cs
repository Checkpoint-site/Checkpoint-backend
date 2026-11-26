namespace Application.Dtos.Moderacao.Requests;

public record AlterarSpoilerPostRequestDto
{
    public required bool TemSpoiler { get; init; }
}
