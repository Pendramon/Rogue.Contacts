using Remora.Results;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Interfaces;

public interface IBusinessService
{
    Task<Result<BusinessDto>> CreateBusinessAsync(CreateBusinessDto createBusinessModel, CancellationToken ct = default);

    Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessDto getBusinessModel, CancellationToken ct = default);

    Task<Result<BusinessRoleDto>> CreateRoleAsync(CreateBusinessRoleDto createRoleModel, CancellationToken ct = default);

    Task<Result<IEnumerable<BusinessRoleDto>>> GetAllRolesAsync(GetAllBusinessRolesDto getAllRolesModel, CancellationToken ct = default);

    Task<Result<BusinessRoleDto>> UpdateRoleAsync(UpdateBusinessRoleDto updateRoleModel, CancellationToken ct = default);

    Task<Result> DeleteRoleAsync(DeleteBusinessRoleDto deleteRoleModel, CancellationToken ct = default);

    Task<Result> DeleteBusinessAsync(DeleteBusinessDto deleteBusinessModel, CancellationToken ct = default);
}
