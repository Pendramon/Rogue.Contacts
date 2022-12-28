using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Remora.Results;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;

namespace Rogue.Contacts.Service;

public sealed class ValidationService : IValidationService
{
    private readonly IServiceProvider serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<AggregateError<ArgumentInvalidError>?> ValidateAsync<T>(T model, CancellationToken ct = default)
    {
        var validator = serviceProvider.GetRequiredService<IValidator<T>>();
        var validationResult = await validator.ValidateAsync(model, ct);
        if (validationResult.IsValid)
        {
            return null;
        }

        return new AggregateError<ArgumentInvalidError>(
            validationResult.Errors
                .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                .ToList());
    }
}
