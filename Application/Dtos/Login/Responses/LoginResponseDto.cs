namespace Application.Dtos.Login.Responses;

public record LoginResponseDto
{
    public required string Token { get; init; }
}
