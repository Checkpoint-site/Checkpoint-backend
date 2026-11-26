namespace Domain.Dtos.Perfis.Commands;

public record AtualizarExperienciaCommandDto
{
    public required int IdUsuario { get; init; }
    public required int NivelGamer { get; init; }
    public required int PontosExperiencia { get; init; }
    public required int LimiteListas { get; init; }
}
