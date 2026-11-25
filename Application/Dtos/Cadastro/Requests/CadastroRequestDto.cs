using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Cadastro.Requests;

public record CadastroRequestDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public required string Nome { get; init; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public required string Senha { get; init; }

    [Required(ErrorMessage = "O código de verificação é obrigatório.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "O código de verificação deve conter exatamente 6 dígitos numéricos.")]
    public required string Codigo { get; init; }

    public string TipoPerfil { get; init; } = "Regular";
    public string? CodigoCredencial { get; init; }
    public string? SetorModeracao { get; init; }
    public string? FotoPerfil { get; init; }
}
