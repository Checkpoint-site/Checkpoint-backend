namespace Application.Dtos.Catalogo.Requests;

public record ObterDetalhesJogoRequestDto
{
    public required int SteamAppId { get; init; }
}
