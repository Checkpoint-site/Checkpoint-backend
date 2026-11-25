using Domain.Dtos.Catalogo;

namespace Application.Dtos.Catalogo.Responses;

public record UsuarioPopularResponseDto
{
    public required int IdUsuario { get; init; }
    public required string NomeUsuario { get; init; }
    public double PontuacaoPopularidade { get; init; }
    public ulong PresencaPost { get; init; }
    public ulong PresencaComentario { get; init; }
    public ulong PresencaFavoritos { get; init; }
    public ulong CurtidasRecebidas { get; init; }
    public ulong QuantidadeSeguidores { get; init; }
    public required IReadOnlyList<RelatoResumoComunidadeDto> UltimosRelatos { get; init; }
}
