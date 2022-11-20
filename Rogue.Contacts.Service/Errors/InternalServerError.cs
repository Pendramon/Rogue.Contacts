using Remora.Results;

namespace Rogue.Contacts.Service.Errors;

public sealed record InternalServerError(
    string Message = "An internal server error has occurred.") : ResultError(Message);