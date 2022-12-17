using Remora.Results;

namespace Rogue.Contacts.Service.Errors;

public sealed record ForbiddenError(
    string Message = "You do not have permission to perform this action.") : ResultError(Message);