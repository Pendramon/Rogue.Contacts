using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remora.Results;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.WebAPI.V1.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public sealed class UserController : Controller
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [AllowAnonymous]
    [Route("register")]
    [HttpPost]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterDto model, CancellationToken ct = default)
    {
        var registerResult = await userService.RegisterAsync(model, ct);

        if (!registerResult.IsSuccess)
        {
            return registerResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => StatusCode(400, error),
                AggregateError<ArgumentConflictError> error => Conflict(error),
                _ => StatusCode(500),
            };
        }

        return Ok(registerResult.Entity);
    }

    [AllowAnonymous]
    [Route("login")]
    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginDto model, CancellationToken ct = default)
    {
        var loginResult = await userService.LoginAsync(model, ct);

        if (!loginResult.IsSuccess)
        {
            return loginResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => Unauthorized(error),
                _ => StatusCode(500),
            };
        }

        return Ok(loginResult.Entity);
    }
}
