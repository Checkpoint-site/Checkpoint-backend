using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Moderacao.Requests;

public record CriarSolicitacaoModeradorRequestDto
{
    [Required(ErrorMessage = "O motivo da solicitação é obrigatório.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "O motivo deve ter entre 10 e 500 caracteres.")]
    public required string Motivo { get; init; }
}
