using Domain.Dtos.Catalogo;

namespace Application.Dtos.Catalogo.Responses;

public record TendenciaGeneroItemResponseDto
{
    public required string Genero { get; init; }
    public double PontuacaoGlobal { get; init; }
    public double? PontuacaoUsuario { get; init; }
    public ulong PresencaFavoritosGlobal { get; init; }
    public ulong PresencaPretendeJogarGlobal { get; init; }
    public ulong PresencaPostGlobal { get; init; }
    public ulong PresencaComentarioGlobal { get; init; }
    public ulong QuantidadeCurtidaGlobal { get; init; }
}

public record TendenciasGenerosResponseDto
{
    public bool PersonalizadoParaUsuario { get; init; }
    public required IReadOnlyList<TendenciaGeneroItemResponseDto> Generos { get; init; }
}

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
