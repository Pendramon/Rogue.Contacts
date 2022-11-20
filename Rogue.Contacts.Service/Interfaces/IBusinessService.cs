using Remora.Results;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Interfaces;

public interface IBusinessService
{
    Task<Result<Business>> CreateBusinessAsync(CreateBusinessRequestDto createBusinessModel, CancellationToken ct);

    Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessRequestDto getBusinessModel, CancellationToken ct);

    Task<Result> DeleteBusinessAsync(DeleteBusinessRequestDto deleteBusinessModel, CancellationToken ct);
}
