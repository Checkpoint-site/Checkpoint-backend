using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Cadastro.Requests;

public record SolicitarCodigoRequestDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    public required string Email { get; init; }
}
