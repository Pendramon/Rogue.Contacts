using Remora.Results;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Interfaces;

public interface IBusinessService
{
    Task<Result<BusinessDto>> CreateBusinessAsync(CreateBusinessRequestDto createBusinessModel, CancellationToken ct);

    Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessRequestDto getBusinessModel, CancellationToken ct);

    Task<Result> CreateRoleAsync(CreateRoleRequestDto createRoleModel, CancellationToken ct);

    Task<Result> AddPermissionsToRole(AddPermissionsToRoleRequestDto addPermissionToRoleModel, CancellationToken ct);

    Task<Result> DeleteBusinessAsync(DeleteBusinessRequestDto deleteBusinessModel, CancellationToken ct);
}
