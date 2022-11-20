using Remora.Results;

namespace Rogue.Contacts.Service.Errors;

public sealed record InternalServerError(
    string? Message = null) : ResultError($"{Message}");