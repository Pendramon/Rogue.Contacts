using Remora.Results;

namespace Rogue.Contacts.Service.Errors;

/// <summary>
/// Represents a conflict error arising from an argument.
/// </summary>
/// <param name="Name">The name of the argument.</param>
/// <param name="Message">The error message.</param>
public sealed record ArgumentConflictError
(
    string Name,
    string Message) : ArgumentError(Name, Message);
