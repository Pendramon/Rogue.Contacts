using EnumsNET;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Remora.Results;
using Rogue.Contacts.Data;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.Shared.Models;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service;

public sealed class BusinessService : IBusinessService
{
    private readonly MongoContext context;
    private readonly IUserResolver userResolver;
    private readonly IValidator<CreateBusinessRequestDto> createBusinessModelValidator;
    private readonly IValidator<GetBusinessRequestDto> getBusinessModelValidator;
    private readonly IValidator<CreateRoleRequestDto> createRoleModelValidator;
    private readonly IValidator<AddPermissionsToRoleRequestDto> addPermissionToRoleModelValidator;
    private readonly IValidator<DeleteBusinessRequestDto> deleteBusinessModelValidator;

    public BusinessService(
        MongoContext context,
        IUserResolver userResolver,
        IValidator<CreateBusinessRequestDto> createBusinessModelValidator,
        IValidator<GetBusinessRequestDto> getBusinessModelValidator,
        IValidator<CreateRoleRequestDto> createRoleModelValidator,
        IValidator<AddPermissionsToRoleRequestDto> addPermissionToRoleModelValidator,
        IValidator<DeleteBusinessRequestDto> deleteBusinessModelValidator)
    {
        this.context = context;
        this.userResolver = userResolver;
        this.createBusinessModelValidator = createBusinessModelValidator;
        this.getBusinessModelValidator = getBusinessModelValidator;
        this.createRoleModelValidator = createRoleModelValidator;
        this.addPermissionToRoleModelValidator = addPermissionToRoleModelValidator;
        this.deleteBusinessModelValidator = deleteBusinessModelValidator;
    }

    public async Task<Result<BusinessDto>> CreateBusinessAsync(CreateBusinessRequestDto createBusinessModel, CancellationToken ct)
    {
        var validationResult = await createBusinessModelValidator.ValidateAsync(createBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var businessNameIsTaken = await context.Businesses.AsQueryable().AnyAsync(
            b => b.Name == createBusinessModel.BusinessName && b.OwnerId == userResolver.GetUserId(), cancellationToken: ct);

        if (businessNameIsTaken)
        {
            return new ArgumentConflictError("Name", "Business with this name already exists.");
        }

        var businessOwner =
            await context.Users.AsQueryable().FirstOrDefaultAsync(u => u.Id == userResolver.GetUserId(), cancellationToken: ct);

        var business = new Business(createBusinessModel.BusinessName, userResolver.GetUserId(), businessOwner.Username, DateTime.UtcNow, Array.Empty<Role>());

        await context.Businesses.InsertOneAsync(business, cancellationToken: ct);

        return new BusinessDto(business.Name, business.OwnerUsername, business.CreatedAt, business.Roles.Select(r => new RoleDto(r.Name, r.Permissions)).ToList());
    }

    public async Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessRequestDto getBusinessModel, CancellationToken ct)
    {
        var validationResult = await getBusinessModelValidator.ValidateAsync(getBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        Business business;

        if (getBusinessModel.BusinessId is not null)
        {
            business = await context.Businesses.AsQueryable()
                .FirstOrDefaultAsync(b => b.Id == ObjectId.Parse(getBusinessModel.BusinessId), cancellationToken: ct);
        }
        else if (!string.IsNullOrWhiteSpace(getBusinessModel.OwnerUsername) && !string.IsNullOrWhiteSpace(getBusinessModel.BusinessName))
        {
            business = await context.Businesses.AsQueryable()
                .FirstOrDefaultAsync(b => b.OwnerUsername == getBusinessModel.OwnerUsername && b.Name == getBusinessModel.BusinessName, cancellationToken: ct);
        }
        else
        {
            return new InvalidOperationError("Could not get business with the requested data as the data is either not distinctive or supported.");
        }

        // TODO: Check if user has permission to see confidential client information for said business.
        if (business is null)
        {
            return new NotFoundError($"The business was not found.");
        }

        return new BusinessDto(
            business.Name,
            business.OwnerUsername,
            business.CreatedAt,
            business.Roles.Select(r => new RoleDto(r.Name, r.Permissions))
                .ToList());
    }

    public async Task<Result> CreateRoleAsync(CreateRoleRequestDto createRoleModel, CancellationToken ct)
    {
        var validationResult = await createRoleModelValidator.ValidateAsync(createRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.AsQueryable()
            .FirstOrDefaultAsync(b => b.OwnerUsername == createRoleModel.Owner && b.Name == createRoleModel.BusinessName, cancellationToken: ct);

        // TODO: Check if employee has permission to manage roles for said business.
        if (business is null || userResolver.GetUserId() != business.OwnerId)
        {
            return new NotFoundError("The business was not found.");
        }

        if (business.Roles.Any(r => r.Name == createRoleModel.RoleName))
        {
            return new AlreadyExistsError("A role with this name already exists.");
        }

        var update = Builders<Business>.Update.Push(b => b.Roles, new Role(createRoleModel.RoleName, Array.Empty<Permission>()));
        var result = await context.Businesses.UpdateOneAsync(x => x.Id == business.Id, update, cancellationToken: ct);

        return result.IsAcknowledged ? Result.FromSuccess() : new InternalServerError();
    }

    public async Task<Result> AddPermissionsToRole(
        AddPermissionsToRoleRequestDto addPermissionsToRoleModel,
        CancellationToken ct)
    {
        var validationResult = await addPermissionToRoleModelValidator.ValidateAsync(addPermissionsToRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.AsQueryable()
            .FirstOrDefaultAsync(b => b.OwnerUsername == addPermissionsToRoleModel.Owner && b.Name == addPermissionsToRoleModel.Business, cancellationToken: ct);

        // TODO: Check if employee has permission to manage roles for said business.
        if (business is null || userResolver.GetUserId() != business.OwnerId)
        {
            return new NotFoundError("The business was not found.");
        }

        var role = business.Roles.FirstOrDefault(r => r.Name == addPermissionsToRoleModel.Role);

        if (role is null)
        {
            return new NotFoundError("The role was not found.");
        }

        var filterBuilder = Builders<Business>.Filter;
        var filter = filterBuilder.Eq(b => b.Id, business.Id) &
                     filterBuilder.ElemMatch(b => b.Roles, r => r.Name == role.Name);

        var convertedPermissions = new List<Permission>();
        foreach (var permission in addPermissionsToRoleModel.Permissions)
        {
            if (!Enums.TryToObject(permission, out Permission convertedPermission, EnumValidation.IsDefined))
            {
                return new AggregateError<ArgumentInvalidError>(new ArgumentInvalidError(
                    nameof(addPermissionsToRoleModel.Permissions),
                    $"{permission} is not a valid permission."));
            }

            convertedPermissions.Add(convertedPermission);
        }

        var update = Builders<Business>.Update.AddToSetEach(b => b.Roles[-1].Permissions, convertedPermissions);
        var result = await context.Businesses.UpdateOneAsync(filter, update, cancellationToken: ct);

        return result.IsAcknowledged ? Result.FromSuccess() : new InternalServerError();
    }

    public async Task<Result> DeleteBusinessAsync(DeleteBusinessRequestDto deleteBusinessModel, CancellationToken ct)
    {
        var validationResult = await deleteBusinessModelValidator.ValidateAsync(deleteBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var businessExists = await context.Businesses.AsQueryable().AnyAsync(b => b.OwnerUsername == deleteBusinessModel.Owner && b.Name == deleteBusinessModel.Business, cancellationToken: ct);

        if (!businessExists)
        {
            return new NotFoundError($"The business was not found.");
        }

        var deleteResult = await context.Businesses.DeleteOneAsync(
            b => b.OwnerUsername == deleteBusinessModel.Owner && b.Name == deleteBusinessModel.Business, cancellationToken: ct);

        return deleteResult.IsAcknowledged ? Result.FromSuccess() : new InternalServerError();
    }
}
