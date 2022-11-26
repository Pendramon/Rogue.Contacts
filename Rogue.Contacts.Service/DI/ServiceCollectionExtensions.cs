using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rogue.Contacts.Data;
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
        services.AddSingleton<MongoContext>();
        services.AddTransient<IHashFunction, BCryptFunction>();
        services.AddSingleton<IHashService, HashService>();
        services.AddScoped<IValidator<UserRegisterDto>, UserRegisterModelValidator>();
        services.AddScoped<IValidator<UserLoginDto>, UserLoginModelValidator>();
        services.AddScoped<IValidator<CreateBusinessDto>, CreateBusinessModelValidator>();
        services.AddScoped<IValidator<GetBusinessDto>, GetBusinessModelValidator>();
        services.AddScoped<IValidator<CreateRoleDto>, CreateRoleModelValidator>();
        services.AddScoped<IValidator<UpdateRoleDto>, UpdateRoleModelValidator>();
        services.AddScoped<IValidator<DeleteRoleDto>, DeleteRoleModelValidator>();
        services.AddScoped<IValidator<DeleteBusinessDto>, DeleteBusinessModelValidator>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBusinessService, BusinessService>();

        return services;
    }
}
