using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Estantes.Requests;

public record AtualizarFavoritosConfigRequestDto
{
    public int? JogoDestaqueSteamAppId { get; init; }

    public bool ExibirNoTopoPerfil { get; init; } = true;

    [StringLength(300, ErrorMessage = "A observação de destaque deve ter no máximo 300 caracteres.")]
    public string? ObservacaoDestaque { get; init; }
}

public record AtualizarPretendeJogarConfigRequestDto
{
    [Range(1, 5, ErrorMessage = "O nível de prioridade deve estar entre 1 e 5.")]
    public int NivelPrioridade { get; init; } = 1;

    public bool NotificarEmPromocao { get; init; } = true;

    public int? MetaAnoConclusao { get; init; }
}
