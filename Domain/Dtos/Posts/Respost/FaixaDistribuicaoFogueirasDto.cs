namespace Domain.Dtos.Posts.Respost;

public record FaixaDistribuicaoFogueirasDto
{
    public required int Estrelas { get; init; }
    public required int Quantidade { get; init; }
    public required int Percentual { get; init; }
}
