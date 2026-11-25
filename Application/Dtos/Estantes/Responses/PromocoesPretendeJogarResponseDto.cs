namespace Application.Dtos.Estantes.Responses;

public record PromocoesPretendeJogarResponseDto
{
    public bool NotificacoesAtivas { get; init; }
    public int TotalEmPromocao { get; init; }
    public required IReadOnlyList<PromocaoJogoEstanteResponseDto> JogosEmPromocao { get; init; }
}
