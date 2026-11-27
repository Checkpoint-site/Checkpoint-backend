namespace Domain.Dtos.Posts.Respost;

public record ResultadoCurtidaRespostDto
{
    public required int IdAlvo { get; init; }
    public required bool Curtido { get; init; }
    public required int QuantidadeCurtidas { get; init; }
}
