using Application.Dtos.Posts.Responses;

namespace Application.Dtos.Diario.Responses;

public record ResumoBibliotecaUsuarioResponseDto
{
    public int Jogados { get; init; }
    public int QueroJogar { get; init; }
    public int Favoritos { get; init; }
    public int Reviews { get; init; }
    public double MediaFogueirasUsuario { get; init; }
}

public record BibliotecaUsuarioResponseDto
{
    public required int IdUsuario { get; init; }
    public required ResumoBibliotecaUsuarioResponseDto Resumo { get; init; }
    public required IReadOnlyList<RegistroJogoUsuarioResponseDto> Registros { get; init; }
    public required IReadOnlyList<PostResponseDto> ReviewsUsuario { get; init; }
}
