using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remora.Results;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
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
        var registerResult = await businessService.CreateBusinessAsync(createBusinessModel, ct);

        if (!registerResult.IsSuccess)
        {
            return registerResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                ArgumentConflictError error => Conflict(error),
                _ => StatusCode(500),
            };
        }

        return Created(Request.Path.Value + $"\"{createBusinessModel.Name}", registerResult.Entity);
    }

    [HttpGet]
    public async Task<IActionResult> GetBusinessAsync(GetBusinessRequestDto getBusinessModel, CancellationToken ct)
    {
        var getBusinessResult = await businessService.GetBusinessAsync(getBusinessModel, ct);

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

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteBusinessAsync(DeleteBusinessRequestDto deleteBusinessModel, CancellationToken ct)
    {
        var registerResult = await businessService.DeleteBusinessAsync(deleteBusinessModel, ct);

        if (!registerResult.IsSuccess)
        {
            return registerResult.Error switch
            {
                AggregateError<ArgumentInvalidError> error => BadRequest(error),
                NotFoundError error => NotFound(error),
                _ => StatusCode(500),
            };
        }

        return NoContent();
    }
}
