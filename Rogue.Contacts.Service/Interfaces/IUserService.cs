using Remora.Results;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Interfaces;

public interface IUserService
{
    Task<Result<AuthenticationResult>> RegisterAsync(UserRegisterRequestDto userRegisterModel, CancellationToken ct = default);

    Task<Result<AuthenticationResult>> LoginAsync(UserLoginRequestDto userLoginModel, CancellationToken ct = default);
}
