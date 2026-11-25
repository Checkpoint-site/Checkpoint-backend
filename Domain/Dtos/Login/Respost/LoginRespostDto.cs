namespace Domain.Dtos.Login.Response;

public record LoginRespostDto
{
    public required int Id { get; init; }
    public required string Email { get; init; }
    public required string Senha { get; init; }
    public string TipoPerfil { get; init; } = "Regular";
    public DateTime? SuspensoAte { get; init; }
}
