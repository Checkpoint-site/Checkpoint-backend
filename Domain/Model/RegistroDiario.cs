using Domain.Enum;

namespace Domain.Model;

public class RegistroDiario
{
    public int IdRegistroDiario { get; set; }
    public required int IdUsuario { get; set; }
    public required int IdJogo { get; set; }
    public bool Jogado { get; set; }
    public StatusJornada? Status { get; set; }
    public double? NotaFogueiras { get; set; }
    public List<string> Tags { get; set; } = [];
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
