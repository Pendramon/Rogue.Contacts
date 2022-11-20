using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remora.Results;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterRequestDto model, CancellationToken ct = default)
    {
        var registerResult = await this.userService.RegisterAsync(model, ct);

        if (!registerResult.IsSuccess)
        {
            return registerResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => this.StatusCode(400, error),
                AggregateError<ArgumentConflictError> error => this.Conflict(error),
                _ => this.StatusCode(500),
            };
        }

        return this.Ok(registerResult.Entity);
    }

    [AllowAnonymous]
    [Route("login")]
    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequestDto model, CancellationToken ct = default)
    {
        var loginResult = await this.userService.LoginAsync(model, ct);

        if (!loginResult.IsSuccess)
        {
            return loginResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => this.Unauthorized(error),
                _ => this.StatusCode(500),
            };
        }

        return this.Ok(loginResult.Entity);
    }
}
