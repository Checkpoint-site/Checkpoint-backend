using Domain.Dtos.Cadastro.Commands;
using Domain.Dtos.Cadastro.Response;

namespace Domain.Interfaces.Repository;

public interface ICadastroRepository
{
    Task<bool> EmailExisteAsync(string email);
    Task<CadastroRespostDto?> CadastroRepositoryAsync(CadastroCommandsDto command);
}
