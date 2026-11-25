using Domain.Enum;

namespace Domain.Dtos.Catalogo.Commands;

public record BuscarCandidatosLocaisCommandDto
{
    public required TipoBuscaCatalogo TipoBusca { get; init; }
    public required string JogoBuscado { get; init; }
}
