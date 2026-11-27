namespace Application.Dtos.Catalogo.Responses;

public record JogoMetricasLoggerResponseDto
{
    public ulong QuantidadeCurtida { get; init; }
    public ulong PresencaPost { get; init; }
    public ulong PresencaComentario { get; init; }
    public ulong PresencaFavoritos { get; init; }
    public ulong PresencaPretendeJogar { get; init; }
    public double SomaFogueiras { get; init; }
    public ulong TotalAvaliacoes { get; init; }
    public double MediaFogueiras { get; init; }
}
