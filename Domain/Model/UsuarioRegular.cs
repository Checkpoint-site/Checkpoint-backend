namespace Domain.Model;

public class UsuarioRegular : Usuario
{
    public int NivelGamer { get; set; } = 1;
    public int PontosExperiencia { get; set; } = 0;
    public int LimiteListas { get; set; } = 15;
    public string? BiografiaGamer { get; set; }
}
