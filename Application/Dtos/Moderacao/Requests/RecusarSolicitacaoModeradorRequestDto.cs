using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Moderacao.Requests;

public record RecusarSolicitacaoModeradorRequestDto
{
    [Required(ErrorMessage = "O motivo da recusa é obrigatório.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "O motivo da recusa deve ter entre 5 e 500 caracteres.")]
    public required string Motivo { get; init; }
}
