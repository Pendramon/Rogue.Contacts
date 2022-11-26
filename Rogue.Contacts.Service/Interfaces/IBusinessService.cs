using Remora.Results;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Interfaces;

public interface IBusinessService
{
    Task<Result<BusinessDto>> CreateBusinessAsync(CreateBusinessDto createBusinessModel, CancellationToken ct);

    Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessDto getBusinessModel, CancellationToken ct);

    Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleModel, CancellationToken ct);

    Task<Result<RoleDto>> UpdateRoleAsync(UpdateRoleDto updateRoleModel, CancellationToken ct);

    Task<Result> DeleteRoleAsync(DeleteRoleDto deleteRoleModel, CancellationToken ct);

    Task<Result> DeleteBusinessAsync(DeleteBusinessDto deleteBusinessModel, CancellationToken ct);
}
