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
    private readonly IValidator<CreateBusinessRequestDto> createBusinessModelValidator;
    private readonly IValidator<GetBusinessRequestDto> getBusinessModelValidator;
    private readonly IValidator<DeleteBusinessRequestDto> deleteBusinessModelValidator;

    public BusinessService(MongoContext context, IUserResolver userResolver, IValidator<CreateBusinessRequestDto> createBusinessModelValidator, IValidator<GetBusinessRequestDto> getBusinessModelValidator, IValidator<DeleteBusinessRequestDto> deleteBusinessModelValidator)
    {
        this.context = context;
        this.userResolver = userResolver;
        this.createBusinessModelValidator = createBusinessModelValidator;
        this.getBusinessModelValidator = getBusinessModelValidator;
        this.deleteBusinessModelValidator = deleteBusinessModelValidator;
    }

    public async Task<Result<Business>> CreateBusinessAsync(CreateBusinessRequestDto createBusinessModel, CancellationToken ct)
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

        var business = new Business(createBusinessModel.Name, userResolver.GetUserId(), businessOwner.Username, DateTime.UtcNow);

        await context.Businesses.InsertOneAsync(business, cancellationToken: ct);

        return business;
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

        if (business is null)
        {
            return new NotFoundError($"Business was not found.");
        }

        return new BusinessDto(business.Name, business.OwnerUsername);
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

        var businessExists = await context.Businesses.AsQueryable().AnyAsync(b => b.Name == deleteBusinessModel.Name, cancellationToken: ct);

        if (!businessExists)
        {
            return new NotFoundError($"The business you tried to delete was not found.");
        }

        var deleteResult = await context.Businesses.DeleteOneAsync(
            b => b.Name == deleteBusinessModel.Name && b.OwnerId == userResolver.GetUserId(), cancellationToken: ct);

        return deleteResult.IsAcknowledged ? Result.FromSuccess() : new InternalServerError();
    }
}
