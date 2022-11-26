using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Remora.Results;
using Rogue.Contacts.Data;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service;

public sealed class BusinessService : IBusinessService
{
    private readonly MongoContext context;
    private readonly IUserResolver userResolver;
    private readonly IValidator<CreateBusinessDto> createBusinessModelValidator;
    private readonly IValidator<GetBusinessDto> getBusinessModelValidator;
    private readonly IValidator<CreateRoleDto> createRoleModelValidator;
    private readonly IValidator<UpdateRoleDto> updateRoleModelValidator;
    private readonly IValidator<DeleteRoleDto> deleteRoleModelValidator;
    private readonly IValidator<DeleteBusinessDto> deleteBusinessModelValidator;

    public BusinessService(
        MongoContext context,
        IUserResolver userResolver,
        IValidator<CreateBusinessDto> createBusinessModelValidator,
        IValidator<GetBusinessDto> getBusinessModelValidator,
        IValidator<CreateRoleDto> createRoleModelValidator,
        IValidator<UpdateRoleDto> updateRoleModelValidator,
        IValidator<DeleteRoleDto> deleteRoleModelValidator,
        IValidator<DeleteBusinessDto> deleteBusinessModelValidator)
    {
        this.context = context;
        this.userResolver = userResolver;
        this.createBusinessModelValidator = createBusinessModelValidator;
        this.getBusinessModelValidator = getBusinessModelValidator;
        this.createRoleModelValidator = createRoleModelValidator;
        this.updateRoleModelValidator = updateRoleModelValidator;
        this.deleteRoleModelValidator = deleteRoleModelValidator;
        this.deleteBusinessModelValidator = deleteBusinessModelValidator;
    }

    public async Task<Result<BusinessDto>> CreateBusinessAsync(CreateBusinessDto createBusinessModel, CancellationToken ct)
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
            b => b.Name == createBusinessModel.Name && b.OwnerId == userResolver.GetUserId(), cancellationToken: ct);

        if (businessNameIsTaken)
        {
            return new ArgumentConflictError("Name", "Business with this name already exists.");
        }

        var businessOwner =
            await context.Users.AsQueryable().FirstOrDefaultAsync(u => u.Id == userResolver.GetUserId(), cancellationToken: ct);

        var business = new Business(createBusinessModel.Name, userResolver.GetUserId(), businessOwner.Username, DateTime.UtcNow, Array.Empty<Role>());

        await context.Businesses.InsertOneAsync(business, cancellationToken: ct);

        return new BusinessDto(business.OwnerUsername, business.Name, business.CreatedAt, business.Roles.Select(r => new RoleDto(r.Id.ToString(), r.Name, r.Permissions)).ToArray());
    }

    public async Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessDto getBusinessModel, CancellationToken ct)
    {
        var validationResult = await getBusinessModelValidator.ValidateAsync(getBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.AsQueryable()
            .FirstOrDefaultAsync(
                b => b.OwnerUsername == getBusinessModel.Owner && b.Name == getBusinessModel.Name,
                cancellationToken: ct);

        // TODO: Check if user has permission to see confidential client information for said business.
        if (business is null)
        {
            return new NotFoundError("The business was not found.");
        }

        return new BusinessDto(
            business.OwnerUsername,
            business.Name,
            business.CreatedAt,
            business.Roles.Select(r => new RoleDto(r.Id.ToString(), r.Name, r.Permissions)).ToArray());
    }

    public async Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleModel, CancellationToken ct)
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
            .FirstOrDefaultAsync(b => b.OwnerUsername == createRoleModel.Owner && b.Name == createRoleModel.Business, cancellationToken: ct);

        // TODO: Check if employee has permission to manage roles for said business.
        if (business is null || userResolver.GetUserId() != business.OwnerId)
        {
            return new NotFoundError("The business was not found.");
        }

        if (business.Roles.Any(r => r.Name == createRoleModel.Name))
        {
            return new AlreadyExistsError("A role with this name already exists.");
        }

        var role = new Role(createRoleModel.Name, createRoleModel.Permissions?.ToArray());

        var update = Builders<Business>.Update.Push(b => b.Roles, role);
        var result = await context.Businesses.UpdateOneAsync(x => x.Id == business.Id, update, cancellationToken: ct);

        return result.IsAcknowledged ? new RoleDto(role.Id.ToString(), role.Name, role.Permissions) : new InternalServerError();
    }

    public async Task<Result<RoleDto>> UpdateRoleAsync(
        UpdateRoleDto updateRoleModel,
        CancellationToken ct)
    {
        var validationResult = await updateRoleModelValidator.ValidateAsync(updateRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage)).ToList());
        }

        var business = await context.Businesses.AsQueryable()
            .FirstOrDefaultAsync(b => b.OwnerUsername == updateRoleModel.Owner && b.Name == updateRoleModel.Business, cancellationToken: ct);

        // TODO: Check if employee has permission to manage roles for said business.
        if (business is null || userResolver.GetUserId() != business.OwnerId)
        {
            return new NotFoundError("The business was not found.");
        }

        var roleId = ObjectId.Parse(updateRoleModel.RoleId);

        var role = business.Roles.FirstOrDefault(r => r.Id == roleId);

        if (role is null)
        {
            return new NotFoundError("The role was not found.");
        }

        var filterBuilder = Builders<Business>.Filter;
        var filter = filterBuilder.Eq(b => b.Id, business.Id) &
                     filterBuilder.ElemMatch(b => b.Roles, r => r.Id == roleId);

        var updates = new List<UpdateDefinition<Business>>();

        if (updateRoleModel.Name is not null)
        {
            updates.Add(Builders<Business>.Update.Set(b => b.Roles[-1].Name, updateRoleModel.Name));
        }

        if (updateRoleModel.Permissions is not null)
        {
            updates.Add(Builders<Business>.Update.Set(b => b.Roles[-1].Permissions, updateRoleModel.Permissions));
        }

        var update = Builders<Business>.Update.Combine(updates);
        var result = await context.Businesses.UpdateOneAsync(filter, update, cancellationToken: ct);

        return result.IsAcknowledged ? new RoleDto(role.Id.ToString(), updateRoleModel.Name ?? role.Name, updateRoleModel.Permissions ?? role.Permissions) : new InternalServerError();
    }

    public async Task<Result> DeleteRoleAsync(DeleteRoleDto deleteRoleModel, CancellationToken ct)
    {
        var validationResult = await deleteRoleModelValidator.ValidateAsync(deleteRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.AsQueryable().FirstOrDefaultAsync(
            b => b.OwnerUsername == deleteRoleModel.Owner && b.Name == deleteRoleModel.Business, cancellationToken: ct);

        // TODO: Check if employee has permission to manage roles for said business.
        if (business is null || userResolver.GetUserId() != business.OwnerId)
        {
            return new NotFoundError("The business was not found.");
        }

        var roleId = ObjectId.Parse(deleteRoleModel.RoleId);

        if (business.Roles.All(r => r.Id != roleId))
        {
            return new NotFoundError("The role was not found.");
        }

        var filter = Builders<Business>.Filter.Eq(b => b.Id, business.Id);
        var update = Builders<Business>.Update.PullFilter(b => b.Roles, r => r.Id == roleId);

        var result = await context.Businesses.UpdateOneAsync(filter, update, cancellationToken: ct);

        return result.IsAcknowledged ? Result.FromSuccess() : new InternalServerError();
    }

    public async Task<Result> DeleteBusinessAsync(DeleteBusinessDto deleteBusinessModel, CancellationToken ct)
    {
        var validationResult = await deleteBusinessModelValidator.ValidateAsync(deleteBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.AsQueryable().FirstOrDefaultAsync(b => b.OwnerUsername == deleteBusinessModel.Owner && b.Name == deleteBusinessModel.Business, cancellationToken: ct);

        // TODO: Send different message if user can view business.
        if (business is null || userResolver.GetUserId() != business.OwnerId)
        {
            return new NotFoundError("The business was not found.");
        }

        var deleteResult = await context.Businesses.DeleteOneAsync(
            b => b.OwnerUsername == deleteBusinessModel.Owner && b.Name == deleteBusinessModel.Business, cancellationToken: ct);

        return deleteResult.IsAcknowledged ? Result.FromSuccess() : new InternalServerError();
    }
}
