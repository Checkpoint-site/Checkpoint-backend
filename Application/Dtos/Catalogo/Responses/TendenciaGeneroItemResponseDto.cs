namespace Application.Dtos.Catalogo.Responses;

public record TendenciaGeneroItemResponseDto
{
    public required string Genero { get; init; }
    public double Pontuacao { get; init; }
    public ulong PresencaFavoritos { get; init; }
    public ulong PresencaPretendeJogar { get; init; }
    public ulong PresencaPost { get; init; }
    public ulong PresencaComentario { get; init; }
    public ulong QuantidadeCurtida { get; init; }
}
