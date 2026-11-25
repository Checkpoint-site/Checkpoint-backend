namespace Domain.Dtos.Estantes.Respost;

public record PretendeJogarRespostDto : EstanteCatalogoRespostDto
{
    public int NivelPrioridade { get; init; } = 1;
    public bool NotificarEmPromocao { get; init; } = true;
    public int? MetaAnoConclusao { get; init; }
}
