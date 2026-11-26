namespace Domain.Dtos.Perfis.Respost;

public abstract record PerfilUsuarioRespostDto
{
    public int Id { get; init; }
    public required string Nome { get; init; }
    public required string Email { get; init; }
    public bool ContaDeletada { get; init; }
    public DateTime? SuspensoAte { get; init; }
    public string? FotoPerfil { get; init; }
    public int QuantidadeSeguidores { get; init; }
}
