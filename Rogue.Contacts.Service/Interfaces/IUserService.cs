using Remora.Results;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Interfaces;

public interface IUserService
{
    Task<Result<AuthenticationResult>> RegisterAsync(UserRegisterDto userRegisterModel, CancellationToken ct = default);

    Task<Result<AuthenticationResult>> LoginAsync(UserLoginDto userLoginModel, CancellationToken ct = default);
}
