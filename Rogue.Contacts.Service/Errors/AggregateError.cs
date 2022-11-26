using System.Text;
using Remora.Results;

namespace Rogue.Contacts.Service.Errors;

/// <summary>
/// Represents a set of errors produced by an operation.
/// </summary>
/// <typeparam name="T">The error type.</typeparam>
/// <param name="Errors">The errors.</param>
/// <param name="Message">The custom error message, if any.</param>
/// <remarks>Used in place of <see cref="AggregateException"/>.</remarks>
public sealed record AggregateError<T>(IReadOnlyCollection<T> Errors, string Message = "One or more errors occurred.") : ResultError(BuildMessage(Message, Errors))
    where T : IResultError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateError{T}"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The errors.</param>
    public AggregateError(string message, params T[] errors)
        : this(errors, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateError{T}"/> class.
    /// </summary>
    /// <param name="errors">The errors.</param>
    public AggregateError(params T[] errors)
        : this((IReadOnlyCollection<T>)errors)
    {
    }

    private static string BuildMessage(string message, IReadOnlyCollection<T> errors)
    {
        var sb = new StringBuilder(message);
        sb.AppendLine();

        var index = 0;
        foreach (var error in errors)
        {
            sb.Append($"[{index}]: ");
            var errorLines = (error.Message ?? "Unknown").Split('\n');
            foreach (var errorLine in errorLines)
            {
                sb.Append('\t');
                sb.AppendLine(errorLine);
            }

            ++index;
        }

        return sb.ToString();
    }
}