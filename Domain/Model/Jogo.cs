using Domain.Enum;

namespace Domain.Model;

public class Jogo
{
    public int IdJogo { get; set; }
    public required int SteamAppId { get; set; }
    public required string Nome { get; set; }
    public required List<Generos> Generos { get; set; }
    public required string Descricao { get; set; }
    public required string Imagem { get; set; }
    public required string Desenvolvedora { get; set; }
    public required string Publicadora { get; set; }
    public string? DataLancamentoTexto { get; set; }
    public int? AnoLancamento { get; set; }
    public int? NotaMetacritic { get; set; }
    public int TotalRecomendacoes { get; set; }
    public bool Gratuito { get; set; }
    public bool EmBreve { get; set; }
    public DateTime SincronizadoEm { get; set; } = DateTime.UtcNow;
}
