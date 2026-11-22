using Domain.Dtos.Login.Commands;
using Domain.Dtos.Login.Response;

namespace Domain.Interfaces.Repository;

public interface ILoginRepository
{
    Task<LoginRespostDto?> LoginRepositoryAsync(LoginCommandsDto command);
}
