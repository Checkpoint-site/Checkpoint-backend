namespace Application.Dtos.Estantes.Responses;

public record EstantesUsuarioResponseDto
{
    public int IdUsuario { get; init; }
    public required EstanteFavoritosResponseDto Favoritos { get; init; }
    public required EstantePretendeJogarResponseDto PretendeJogar { get; init; }
}
