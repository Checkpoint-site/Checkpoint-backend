using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Estantes.Requests;

public record AtualizarFavoritosConfigRequestDto
{
    public int? JogoDestaqueSteamAppId { get; init; }

    public bool ExibirNoTopoPerfil { get; init; } = true;

    [StringLength(300, ErrorMessage = "A observação de destaque deve ter no máximo 300 caracteres.")]
    public string? ObservacaoDestaque { get; init; }
}
