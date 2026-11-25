using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Estantes.Requests;

public record AtualizarPretendeJogarConfigRequestDto
{
    [Range(1, 5, ErrorMessage = "O nível de prioridade deve estar entre 1 e 5.")]
    public int NivelPrioridade { get; init; } = 1;

    public bool NotificarEmPromocao { get; init; } = true;

    public int? MetaAnoConclusao { get; init; }
}
