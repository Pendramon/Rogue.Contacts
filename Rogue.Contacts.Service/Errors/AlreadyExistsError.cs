using Remora.Results;

namespace Rogue.Contacts.Service.Errors;

public sealed record AlreadyExistsError(
    string Message = "The entity already exists.") : ResultError(Message);