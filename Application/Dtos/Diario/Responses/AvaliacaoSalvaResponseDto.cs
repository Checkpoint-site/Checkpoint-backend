using Application.Dtos.Catalogo.Responses;
using Application.Dtos.Posts.Responses;

namespace Application.Dtos.Diario.Responses;

public record AvaliacaoSalvaResponseDto
{
    public required RegistroJogoUsuarioResponseDto Registro { get; init; }
    public PostResponseDto? ReviewPublicada { get; init; }
    public double MediaFogueirasJogo { get; init; }
    public int TotalAvaliacoesJogo { get; init; }
    public IReadOnlyList<FaixaDistribuicaoFogueirasResponseDto> DistribuicaoFogueiras { get; init; } = [];
}
