using Application.Dtos.Diario.Responses;
using Application.Dtos.Posts.Responses;
using Application.Utils;
using Domain.Dtos.Diario.Respost;
using Domain.Dtos.Posts.Respost;

namespace Application.Mappers;

public static class PostMapper
{
    public static PostResponseDto ToPostResponse(PostDetalheRespostDto dto) => new()
    {
        IdPost = dto.IdPost,
        IdUsuario = dto.IdUsuario,
        AutorNome = dto.NomeAutor,
        AutorHandle = FormatadorComunidadeUtil.GerarHandle(dto.NomeAutor),
        AutorIniciais = FormatadorComunidadeUtil.ExtrairIniciais(dto.NomeAutor),
        AutorFotoPerfil = dto.FotoPerfilAutor,
        IdJogo = dto.IdJogo,
        SteamAppId = dto.SteamAppId,
        JogoTitulo = dto.NomeJogo,
        JogoAno = dto.AnoLancamentoJogo ?? dto.DataCriacao.Year,
        JogoCapaUrl = dto.ImagemJogo,
        DesenvolvedoraJogo = dto.DesenvolvedoraJogo,
        Titulo = dto.Titulo,
        Nota = Math.Round(dto.Fogueiras, 1),
        Comentario = dto.Conteudo,
        Status = StatusJornadaHelper.ParaSlug(dto.StatusJornada),
        Tags = dto.Tags,
        Curtidas = dto.QuantidadeCurtidas,
        TotalComentarios = dto.QuantidadeComentarios,
        ContemSpoiler = dto.TemSpoiler,
        CurtidaPeloUsuario = dto.CurtidoPeloUsuarioAtual,
        DoUsuarioAtual = dto.DoUsuarioAtual,
        TempoAtras = FormatadorComunidadeUtil.FormatarTempoAtras(dto.DataCriacao),
        DataCriacao = dto.DataCriacao
    };

    public static ComentarioResponseDto ToComentarioResponse(ComentarioRespostDto dto) => new()
    {
        IdComentario = dto.IdComentario,
        IdPost = dto.IdPost,
        IdUsuario = dto.IdUsuario,
        AutorNome = dto.NomeAutor,
        AutorHandle = FormatadorComunidadeUtil.GerarHandle(dto.NomeAutor),
        AutorIniciais = FormatadorComunidadeUtil.ExtrairIniciais(dto.NomeAutor),
        AutorFotoPerfil = dto.FotoPerfilAutor,
        Texto = dto.Conteudo,
        Curtidas = dto.QuantidadeCurtidas,
        ContemSpoiler = dto.TemSpoiler,
        CurtidaPeloUsuario = dto.CurtidoPeloUsuarioAtual,
        DoUsuarioAtual = dto.DoUsuarioAtual,
        TempoAtras = FormatadorComunidadeUtil.FormatarTempoAtras(dto.DataCriacao),
        DataCriacao = dto.DataCriacao
    };

    public static CurtidaResponseDto ToCurtidaResponse(ResultadoCurtidaRespostDto dto) => new()
    {
        IdAlvo = dto.IdAlvo,
        Curtido = dto.Curtido,
        QuantidadeCurtidas = dto.QuantidadeCurtidas
    };

    public static RegistroJogoUsuarioResponseDto ToRegistroResponse(RegistroDiarioRespostDto dto) => new()
    {
        IdJogo = dto.IdJogo,
        SteamAppId = dto.SteamAppId,
        JogoTitulo = dto.NomeJogo,
        JogoCapaUrl = dto.ImagemJogo,
        JogoAno = dto.AnoLancamentoJogo ?? dto.DataAtualizacao.Year,
        Desenvolvedora = dto.DesenvolvedoraJogo,
        Generos = dto.GenerosJogo.Select(g => g.ToString()).ToList(),
        NotaMediaJogo = Math.Round(dto.MediaFogueirasJogo, 2),
        TotalAvaliacoesJogo = dto.TotalAvaliacoesJogo,
        Status = StatusJornadaHelper.ParaSlug(dto.Status),
        Jogado = dto.Jogado,
        QueroJogar = dto.QueroJogar,
        Favorito = dto.Favorito,
        Nota = dto.NotaFogueiras.HasValue ? Math.Round(dto.NotaFogueiras.Value, 1) : null,
        Tags = dto.Tags,
        IdPostReview = dto.IdPostReview,
        Comentario = dto.ComentarioReview,
        ContemSpoiler = dto.ContemSpoilerReview,
        AtualizadoEm = dto.DataAtualizacao
    };
}
