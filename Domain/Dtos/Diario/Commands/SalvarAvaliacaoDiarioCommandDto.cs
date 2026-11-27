using Domain.Enum;

namespace Domain.Dtos.Diario.Commands;

public record SalvarAvaliacaoDiarioCommandDto
{
    public required int IdUsuario { get; init; }
    public required int IdJogo { get; init; }
    public StatusJornada? Status { get; init; }
    public double? NotaFogueiras { get; init; }
    public bool? Jogado { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
}
