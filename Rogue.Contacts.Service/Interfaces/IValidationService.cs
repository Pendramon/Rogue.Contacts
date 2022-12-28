using Remora.Results;
using Rogue.Contacts.Service.Errors;

namespace Rogue.Contacts.Service.Interfaces;

public interface IValidationService
{
    Task<AggregateError<ArgumentInvalidError>?> ValidateAsync<T>(T model, CancellationToken ct = default);
}
