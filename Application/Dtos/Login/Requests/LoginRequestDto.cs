using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Login.Requests;

public record LoginRequestDto
{
    [Required(ErrorMessage = "É necessário informar o endereço de Email")]
    [EmailAddress(ErrorMessage = "Informe um endereço de Email válido")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "É necessário informar a senha")]
    public required string Senha { get; init; }
}
