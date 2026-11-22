namespace Domain.Dtos.Login.Commands;

public record LoginCommandsDto
{
    public required string Email { get; init; }
    public required string Senha { get; init; }
}
