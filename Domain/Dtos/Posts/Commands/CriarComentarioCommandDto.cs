namespace Domain.Dtos.Posts.Commands;

public record CriarComentarioCommandDto
{
    public required int IdPost { get; init; }
    public required int IdUsuario { get; init; }
    public required string Conteudo { get; init; }
    public bool TemSpoiler { get; init; }
}
