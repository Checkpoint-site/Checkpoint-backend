namespace Application.Dtos.Estantes.Requests;

public record AlterarJogoEstanteRequestDto
{
    public required string TipoEstante { get; init; }
    public required int SteamAppId { get; init; }
}
