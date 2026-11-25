namespace Domain.Dtos.Catalogo;

public record RelatoResumoComunidadeDto
{
    public required int IdPost { get; init; }
    public required int IdJogo { get; init; }
    public required string NomeJogo { get; init; }
    public required string Titulo { get; init; }
    public required int Fogueiras { get; init; }
    public int QuantidadeCurtidas { get; init; }
    public required DateTime DataCriacao { get; init; }
}

public record UsuarioPopularComunidadeDto
{
    public required int IdUsuario { get; init; }
    public required string NomeUsuario { get; init; }
    public ulong PresencaPost { get; init; }
    public ulong PresencaComentario { get; init; }
    public ulong PresencaFavoritos { get; init; }
    public ulong QuantidadeCurtida { get; init; }
    public ulong CurtidasRecebidas { get; init; }
    public ulong QuantidadeSeguidores { get; init; }
    public required IReadOnlyList<RelatoResumoComunidadeDto> UltimosRelatos { get; init; }
}
