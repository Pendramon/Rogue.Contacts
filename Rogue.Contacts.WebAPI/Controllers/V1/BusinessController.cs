using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remora.Results;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.WebAPI.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]es")]
[ApiVersion("1.0")]
public sealed class BusinessController : ControllerBase
{
    private readonly IBusinessService businessService;

    public BusinessController(IBusinessService businessService)
    {
        this.businessService = businessService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBusinessAsync(CreateBusinessRequestDto createBusinessModel, CancellationToken ct)
    {
        var createBusinessResult = await businessService.CreateBusinessAsync(createBusinessModel, ct);

        if (!createBusinessResult.IsSuccess)
        {
            return createBusinessResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                ArgumentConflictError error => Conflict(error),
                _ => StatusCode(500),
            };
        }

        return Created($"{Request.Path.Value}/{createBusinessResult.Entity.OwnerUsername}/{createBusinessResult.Entity.Name}", createBusinessResult.Entity);
    }

    [HttpGet]
    [Route("{owner}/{business}")]
    [Authorize]
    public async Task<IActionResult> GetBusinessAsync(string owner, string business, CancellationToken ct)
    {
        var getBusinessResult = await businessService.GetBusinessAsync(new GetBusinessRequestDto(owner, business), ct);

        if (!getBusinessResult.IsSuccess)
        {
            return getBusinessResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                InvalidOperationError error => BadRequest(error),
                NotFoundError error => NotFound(error),
                _ => StatusCode(500),
            };
        }

        return Ok(getBusinessResult.Entity);
    }

    // TODO: Make all request bodies consistent.
    [HttpPost]
    [Route("{owner}/{business}/roles")]
    [Authorize]
    public async Task<IActionResult> CreateRoleAsync(string owner, string business, [FromBody] string roleName, CancellationToken ct)
    {
        var createRoleResult = await businessService.CreateRoleAsync(new CreateRoleRequestDto(owner, business, roleName), ct);

        if (!createRoleResult.IsSuccess)
        {
            return createRoleResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                AlreadyExistsError error => Conflict(error),
                _ => StatusCode(500),
            };
        }

        return Ok();
    }

    [HttpPatch]
    [Route("{owner}/{business}/roles/{role}")]
    [Authorize]
    public async Task<IActionResult> AddPermissionToRoleAsync(string owner, string business, string role, [FromBody] IEnumerable<string> permissions, CancellationToken ct)
    {
        var addPermissionsResult = await businessService.AddPermissionsToRole(new AddPermissionsToRoleRequestDto(owner, business, role, permissions), ct);

        if (!addPermissionsResult.IsSuccess)
        {
            return addPermissionsResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                _ => StatusCode(500),
            };
        }

        return Ok();
    }

    [HttpDelete]
    [Route("{owner}/{business}")]
    [Authorize]
    public async Task<IActionResult> DeleteBusinessAsync(string owner, string business, CancellationToken ct)
    {
        var deleteResult = await businessService.DeleteBusinessAsync(new DeleteBusinessRequestDto(owner, business), ct);

        if (!deleteResult.IsSuccess)
        {
            return deleteResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                _ => StatusCode(500),
            };
        }

        return NoContent();
    }
}
