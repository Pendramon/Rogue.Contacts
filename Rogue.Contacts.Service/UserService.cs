using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Remora.Results;
using Rogue.Contacts.Data;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.Service.Cryptography.Interfaces;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service;

public sealed class UserService : IUserService
{
    private readonly ContactsContext context;
    private readonly IConfiguration configuration;
    private readonly IHashService hashService;
    private readonly IValidator<UserRegisterDto> userRegisterModelValidator;
    private readonly IValidator<UserLoginDto> userLoginModelValidator;

    public UserService(ContactsContext context, IConfiguration configuration, IHashService hashService, IValidator<UserRegisterDto> userRegisterModelValidator, IValidator<UserLoginDto> userLoginModelValidator)
    {
        this.context = context;
        this.configuration = configuration;
        this.hashService = hashService;
        this.userRegisterModelValidator = userRegisterModelValidator;
        this.userLoginModelValidator = userLoginModelValidator;
    }

    public async Task<Result<AuthenticationResult>> RegisterAsync(UserRegisterDto userRegisterModel, CancellationToken ct = default)
    {
        var validationResult = await userRegisterModelValidator.ValidateAsync(userRegisterModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(validationResult.Errors.Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage)).ToList());
        }

        var users = await context.Users.Where(u =>
            u.Name.ToLower() == userRegisterModel.Username.ToLower() ||
            u.Email.ToLower() == userRegisterModel.Email.ToLower()).ToListAsync(ct);

        var errors = new List<ArgumentConflictError>();

        if (users.Any(u => string.Equals(u.Name, userRegisterModel.Username, StringComparison.CurrentCultureIgnoreCase)))
        {
            errors.Add(new ArgumentConflictError("Username", "User with this username already exists."));
        }

        if (users.Any(u => string.Equals(u.Email, userRegisterModel.Email, StringComparison.CurrentCultureIgnoreCase)))
        {
            errors.Add(new ArgumentConflictError("Email", "User with this email address already exists."));
        }

        if (errors.Any())
        {
            return new AggregateError<ArgumentConflictError>(errors);
        }

        var user = new User
        {
            Name = userRegisterModel.Username,
            DisplayName = userRegisterModel.DisplayName,
            Email = userRegisterModel.Email.ToLower(),
            PasswordHash = await hashService.ComputeHashAsync(userRegisterModel.Password),
            CreatedAt = DateTime.UtcNow,
        };

        context.Add(user);
        await context.SaveChangesAsync(ct);

        return new AuthenticationResult(GenerateToken(user), new UserDto(user.Name, user.DisplayName, user.Email, user.CreatedAt));
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(UserLoginDto userLoginModel, CancellationToken ct = default)
    {
        var validationResult = await userLoginModelValidator.ValidateAsync(userLoginModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var user = new EmailAddressAttribute().IsValid(userLoginModel.UsernameOrEmail)
            ? await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userLoginModel.UsernameOrEmail.ToLower(), ct)
            : await context.Users.FirstOrDefaultAsync(u => u.Name.ToLower() == userLoginModel.UsernameOrEmail.ToLower(), ct);

        if (user == null)
        {
            return new AggregateError<ArgumentInvalidError>(new ArgumentInvalidError("UsernameOrEmail", "The account does not exist."));
        }

        var result = await hashService.CompareHashAsync(userLoginModel.Password, user.PasswordHash);

        if (!result.Match)
        {
            return new AggregateError<ArgumentInvalidError>(new ArgumentInvalidError("Password", "Incorrect password."));
        }

        if (result.Rehashed)
        {
            user.PasswordHash = result.RehashResult!;
            context.Users.Update(user);
            await context.SaveChangesAsync(ct);
        }

        return new AuthenticationResult(GenerateToken(user), new UserDto(user.Name, user.DisplayName, user.Email, user.CreatedAt));
    }

    private string GenerateToken(User user)
    {
        var symmetricKey = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]);
        var tokenHandler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            }),

            // TODO: Change expiration when refresh tokens are implemented.
            Expires = now.AddDays(7),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(symmetricKey),
                SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
