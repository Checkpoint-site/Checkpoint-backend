using Domain.Enum;
using Domain.Exceptions.Common;

namespace Domain.Model;

public class Post
{
    public int IdPost { get; set; }
    public required int IdUsuario { get; set; }
    public required int IdJogo { get; set; }
    public required string Titulo { get; set; }
    public required int Fogueiras { get; set; }
    public required string Conteudo { get; set; }
    public required List<Generos> TipoPost { get; set; }
    public int QuantidadeCurtidas { get; set; }
    public int QuantidadeComentarios { get; set; }
    public bool TemSpoiler { get; set; } = false;
    public bool PostDeletado { get; set; } = false;
    public required DateTime DataCriacao { get; set; }

    public void AlterarMarcaSpoiler(bool temSpoiler)
    {
        if (PostDeletado)
        {
            throw new ValidationException("Não é possível alterar a marcação de spoiler de um post excluído.");
        }

        TemSpoiler = temSpoiler;
    }
}
