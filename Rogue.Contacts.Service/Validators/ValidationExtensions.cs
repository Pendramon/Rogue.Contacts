using FluentValidation;

namespace Rogue.Contacts.Service.Validators;

#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1615 // Element return value should be documented
#pragma warning disable SA1618 // Generic type parameters should be documented
internal static class ValidationExtensions
{
    /// <summary>
    /// Validates a username based on the application defined rules.
    /// </summary>
    internal static IRuleBuilderOptions<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.MaximumLength(40).Matches(@"^[A-Za-z0-9]+(?:[-_.][A-Za-z0-9]+)*$").WithMessage("Usernames can only consist of alphanumerical characters. You may also use hyphens, underscores and dots as separators.");
    }

    /// <summary>
    /// Validates a business name based on application defined rules.
    /// </summary>
    internal static IRuleBuilderOptions<T, string> BusinessName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.MaximumLength(40).Matches(@"^[A-Za-z0-9]+(?:[-_.][A-Za-z0-9]+)*$").WithMessage("Business names can only consist of alphanumerical characters. You may also use hyphens, underscores and dots as separators.");
    }

    /// <summary>
    /// Validates a role name based on application defined rules.
    /// </summary>
    internal static IRuleBuilderOptions<T, string> RoleName<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.MaximumLength(32);
    }

    /// <summary>
    /// Validates a password based on application defined rules.
    /// </summary>
    internal static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.MinimumLength(8).MaximumLength(256).Matches(@"^[!-~]*$").WithMessage("Passwords can only consist of alphanumerical and special characters. ASCII Range 33 to 126.");
    }
}