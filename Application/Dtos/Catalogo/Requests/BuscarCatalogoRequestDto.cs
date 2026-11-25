namespace Application.Dtos.Catalogo.Requests;

public record BuscarCatalogoRequestDto
{
    public required string JogoBuscado { get; init; }
    public string? Tipo { get; init; }
    public string? Algoritmo { get; init; }
}
