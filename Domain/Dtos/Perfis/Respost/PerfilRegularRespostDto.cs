namespace Domain.Dtos.Perfis.Respost;

public record PerfilRegularRespostDto : PerfilUsuarioRespostDto
{
    public int NivelGamer { get; init; } = 1;
    public int PontosExperiencia { get; init; } = 0;
    public int LimiteListas { get; init; } = 15;
    public string? BiografiaGamer { get; init; }
}
