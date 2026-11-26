namespace Domain.Dtos.Catalogo;

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
    public bool PossuiFotoPerfil { get; init; }
    public required IReadOnlyList<RelatoResumoComunidadeDto> UltimosRelatos { get; init; }
}
