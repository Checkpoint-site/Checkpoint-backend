namespace Domain.Dtos.Cadastro.Response;

public record CadastroRespostDto
{
    public required int Id { get; init; }
    public required string Nome { get; init; }
    public required string Email { get; init; }
}
