namespace Application.Dtos.Catalogo.Responses;

public record FaixaDistribuicaoFogueirasResponseDto
{
    public required int Estrelas { get; init; }
    public required int Quantidade { get; init; }
    public required int Percentual { get; init; }
}
