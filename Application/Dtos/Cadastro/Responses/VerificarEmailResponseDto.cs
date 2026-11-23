namespace Application.Dtos.Cadastro.Responses;

public record VerificarEmailResponseDto
{
    public required bool Disponivel { get; init; }
}
