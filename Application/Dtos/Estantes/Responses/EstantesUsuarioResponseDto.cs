using Application.Dtos.Catalogo.Responses;

namespace Application.Dtos.Estantes.Responses;

public record EstanteFavoritosResponseDto
{
    public int IdCatalogo { get; init; }
    public bool ExibirNoTopoPerfil { get; init; }
    public int? JogoDestaquePerfilId { get; init; }
    public string? ObservacaoDestaque { get; init; }
    public int TotalJogos { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public required IReadOnlyList<JogoResumoResponseDto> Jogos { get; init; }
}

public record EstantePretendeJogarResponseDto
{
    public int IdCatalogo { get; init; }
    public int NivelPrioridade { get; init; }
    public bool NotificarEmPromocao { get; init; }
    public int? MetaAnoConclusao { get; init; }
    public int TotalJogos { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public required IReadOnlyList<JogoResumoResponseDto> Jogos { get; init; }
}

public record EstantesUsuarioResponseDto
{
    public int IdUsuario { get; init; }
    public required EstanteFavoritosResponseDto Favoritos { get; init; }
    public required EstantePretendeJogarResponseDto PretendeJogar { get; init; }
}

public record PromocaoJogoEstanteResponseDto
{
    public required int IdJogo { get; init; }
    public required int SteamAppId { get; init; }
    public required string Nome { get; init; }
    public required string Imagem { get; init; }
    public int? PrecoOriginalCentavos { get; init; }
    public int? PrecoFinalCentavos { get; init; }
    public required int PercentualDesconto { get; init; }
    public required DateTime ConsultadoEm { get; init; }
}

public record PromocoesPretendeJogarResponseDto
{
    public bool NotificacoesAtivas { get; init; }
    public int TotalEmPromocao { get; init; }
    public required IReadOnlyList<PromocaoJogoEstanteResponseDto> JogosEmPromocao { get; init; }
}
