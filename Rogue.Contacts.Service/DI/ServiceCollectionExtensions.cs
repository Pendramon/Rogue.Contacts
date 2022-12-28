using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rogue.Contacts.Service.Cryptography;
using Rogue.Contacts.Service.Cryptography.Interfaces;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.Service.Validators;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRogueContacts(this IServiceCollection services)
    {
        services.AddTransient<IHashFunction, BCryptFunction>();
        services.AddSingleton<IHashService, HashService>();
        services.AddScoped<BusinessAuthorizationService>();
        services.AddScoped<OrganizationAuthorizationService>();
        services.AddScoped<IValidator<UserRegisterDto>, UserRegisterModelValidator>();
        services.AddScoped<IValidator<UserLoginDto>, UserLoginModelValidator>();
        services.AddScoped<IValidator<CreateBusinessDto>, CreateBusinessModelValidator>();
        services.AddScoped<IValidator<GetBusinessDto>, GetBusinessModelValidator>();
        services.AddScoped<IValidator<CreateBusinessRoleDto>, CreateBusinessRoleModelValidator>();
        services.AddScoped<IValidator<GetAllBusinessRolesDto>, GetAllBusinessRolesModelValidator>();
        services.AddScoped<IValidator<UpdateBusinessRoleDto>, UpdateBusinessRoleModelValidator>();
        services.AddScoped<IValidator<DeleteBusinessRoleDto>, DeleteBusinessRoleModelValidator>();
        services.AddScoped<IValidator<DeleteBusinessDto>, DeleteBusinessModelValidator>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBusinessService, BusinessService>();

        return services;
    }
}
