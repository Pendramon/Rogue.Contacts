using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remora.Results;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;
using Rogue.Contacts.WebAPI.V1.Models;

namespace Rogue.Contacts.WebAPI.V1.Controllers;

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
    public async Task<IActionResult> CreateBusinessAsync(CreateBusinessDto createBusinessModel, CancellationToken ct)
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

        return Created($"{Request.Path.Value}/{createBusinessResult.Entity.Owner}/{createBusinessResult.Entity.Name}", createBusinessResult.Entity);
    }

    [HttpGet]
    [Route("{owner}/{business}")]
    [Authorize]
    public async Task<IActionResult> GetBusinessAsync(string owner, string business, CancellationToken ct)
    {
        var getBusinessResult = await businessService.GetBusinessAsync(new GetBusinessDto(owner, business), ct);

        if (!getBusinessResult.IsSuccess)
        {
            return getBusinessResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                _ => StatusCode(500),
            };
        }

        return Ok(getBusinessResult.Entity);
    }

    [HttpPost]
    [Route("{owner}/{business}/roles")]
    [Authorize]
    public async Task<IActionResult> CreateRoleAsync(string owner, string business, [FromBody] CreateBusinessRoleRequestDto model, CancellationToken ct)
    {
        var createRoleResult = await businessService.CreateRoleAsync(new CreateBusinessRoleDto(owner, business, model.Name, model.Permissions), ct);

        if (!createRoleResult.IsSuccess)
        {
            return createRoleResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                ForbiddenError error => Forbid(),
                ArgumentConflictError error => StatusCode(403, error),
                _ => StatusCode(500),
            };
        }

        return StatusCode(201, createRoleResult.Entity);
    }

    [HttpGet]
    [Route("{owner}/{business}/roles")]
    [Authorize]
    public async Task<IActionResult> GetAllRolesAsync(string owner, string business, CancellationToken ct)
    {
        var getBusinessResult = await businessService.GetAllRolesAsync(new GetAllBusinessRolesDto(owner, business), ct);

        if (!getBusinessResult.IsSuccess)
        {
            return getBusinessResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                ForbiddenError error => StatusCode(403, error),
                _ => StatusCode(500),
            };
        }

        return Ok(getBusinessResult.Entity);
    }

    [HttpPatch]
    [Route("{owner}/{business}/roles/{roleId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateRoleAsync(string owner, string business, int roleId, [FromBody] UpdateBusinessRoleRequestDto model, CancellationToken ct)
    {
        var addPermissionsResult = await businessService.UpdateRoleAsync(new UpdateBusinessRoleDto(owner, business, roleId, model.Name, model.Permissions), ct);

        if (!addPermissionsResult.IsSuccess)
        {
            return addPermissionsResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                ForbiddenError error => StatusCode(403, error),
                _ => StatusCode(500),
            };
        }

        return Ok(addPermissionsResult.Entity);
    }

    [HttpDelete]
    [Route("{owner}/{business}/roles/{roleId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteRoleAsync(string owner, string business, int roleId, CancellationToken ct)
    {
        var deleteResult = await businessService.DeleteRoleAsync(new DeleteBusinessRoleDto(owner, business, roleId), ct);

        if (!deleteResult.IsSuccess)
        {
            return deleteResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                ForbiddenError error => StatusCode(403, error),
                _ => StatusCode(500),
            };
        }

        return NoContent();
    }

    [HttpDelete]
    [Route("{owner}/{business}")]
    [Authorize]
    public async Task<IActionResult> DeleteBusinessAsync(string owner, string business, CancellationToken ct)
    {
        var deleteResult = await businessService.DeleteBusinessAsync(new DeleteBusinessDto(owner, business), ct);

        if (!deleteResult.IsSuccess)
        {
            return deleteResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                ForbiddenError error => StatusCode(403, error),
                _ => StatusCode(500),
            };
        }

        return NoContent();
    }
}
