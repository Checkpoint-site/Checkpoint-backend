namespace Application.Dtos.Diario.Requests;

public record SalvarAvaliacaoJogoRequestDto
{
    public required double Nota { get; init; }
    public string? Status { get; init; }
    public string? Comentario { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
    public bool ContemSpoiler { get; init; }
    public bool? Favorito { get; init; }
}
