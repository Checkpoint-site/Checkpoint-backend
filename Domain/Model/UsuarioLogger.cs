namespace Domain.Model;

public class UsuarioLogger : EngajamentoLogger
{
    public required int IdUsuario { get; set; }
    public ulong CurtidasRecebidas { get; set; }
    public ulong QuantidadeSeguidores { get; set; }
}
