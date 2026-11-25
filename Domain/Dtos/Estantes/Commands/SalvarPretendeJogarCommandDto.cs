namespace Domain.Dtos.Estantes.Commands;

public record SalvarPretendeJogarCommandDto
{
    public required int IdUsuario { get; init; }
    public required IReadOnlyList<int> Jogos { get; init; }
    public int NivelPrioridade { get; init; } = 1;
    public bool NotificarEmPromocao { get; init; } = true;
    public int? MetaAnoConclusao { get; init; }
}
