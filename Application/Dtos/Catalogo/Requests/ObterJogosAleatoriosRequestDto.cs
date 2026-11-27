namespace Application.Dtos.Catalogo.Requests;

public record ObterJogosAleatoriosRequestDto
{
    public int Limite { get; init; } = 12;
    public string? Algoritmo { get; init; }
    public bool ForcarFallbackSteam { get; init; } = false;
}
