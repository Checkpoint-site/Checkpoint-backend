namespace Domain.Model;

public class PretendeJogar : Catalogo
{
    public int NivelPrioridade { get; set; } = 1;
    public bool NotificarEmPromocao { get; set; } = true;
    public int? MetaAnoConclusao { get; set; }
}
