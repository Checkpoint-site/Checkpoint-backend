namespace Application.Dtos.Catalogo.Requests;

public record ObterUsuariosPopularesRequestDto
{
    public int Limite { get; init; } = 10;
}
