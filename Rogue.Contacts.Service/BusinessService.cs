using FastEnumUtility;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Rogue.Contacts.Data;
using Rogue.Contacts.Data.Model;
using Rogue.Contacts.Service.Errors;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.Shared;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service;

public sealed class BusinessService : IBusinessService
{
    // TODO: Figure out what the error messages should contain and figure out better place to put them.
    private const string BusinessNotFoundMessage = "The business was not found.";
    private const string BusinessAlreadyExistsMessage = "Business with this name already exists.";
    private const string BusinessRoleNotFoundMessage = "The business role was not found.";
    private const string BusinessRoleAlreadyExistsMessage = "Business role with this name already exists.";
    private readonly ContactsContext context;
    private readonly IUserResolver userResolver;
    private readonly BusinessAuthorizationService businessAuthorizationService;
    private readonly OrganizationAuthorizationService organizationAuthorizationService;
    private readonly IValidator<CreateBusinessDto> createBusinessModelValidator;
    private readonly IValidator<GetBusinessDto> getBusinessModelValidator;
    private readonly IValidator<GetAllBusinessRolesDto> getAllRolesModelValidator;
    private readonly IValidator<CreateBusinessRoleDto> createRoleModelValidator;
    private readonly IValidator<UpdateBusinessRoleDto> updateRoleModelValidator;
    private readonly IValidator<DeleteBusinessRoleDto> deleteRoleModelValidator;
    private readonly IValidator<DeleteBusinessDto> deleteBusinessModelValidator;

    public BusinessService(
        ContactsContext context,
        IUserResolver userResolver,
        BusinessAuthorizationService businessAuthorizationService,
        OrganizationAuthorizationService organizationAuthorizationService,
        IValidator<CreateBusinessDto> createBusinessModelValidator,
        IValidator<GetBusinessDto> getBusinessModelValidator,
        IValidator<GetAllBusinessRolesDto> getAllRolesModelValidator,
        IValidator<CreateBusinessRoleDto> createRoleModelValidator,
        IValidator<UpdateBusinessRoleDto> updateRoleModelValidator,
        IValidator<DeleteBusinessRoleDto> deleteRoleModelValidator,
        IValidator<DeleteBusinessDto> deleteBusinessModelValidator)
    {
        this.context = context;
        this.userResolver = userResolver;
        this.businessAuthorizationService = businessAuthorizationService;
        this.organizationAuthorizationService = organizationAuthorizationService;
        this.createBusinessModelValidator = createBusinessModelValidator;
        this.getBusinessModelValidator = getBusinessModelValidator;
        this.getAllRolesModelValidator = getAllRolesModelValidator;
        this.createRoleModelValidator = createRoleModelValidator;
        this.updateRoleModelValidator = updateRoleModelValidator;
        this.deleteRoleModelValidator = deleteRoleModelValidator;
        this.deleteBusinessModelValidator = deleteBusinessModelValidator;
    }

    public async Task<Result<BusinessDto>> CreateBusinessAsync(CreateBusinessDto createBusinessModel, CancellationToken ct = default)
    {
        var validationResult = await createBusinessModelValidator.ValidateAsync(createBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var ownerId = userResolver.GetUserId();

        var businessNameIsTaken =
            await context.Businesses.AnyAsync(b => b.Name == createBusinessModel.Name && b.OwnerId == ownerId, ct);

        if (businessNameIsTaken)
        {
            return new ArgumentConflictError(
                nameof(createBusinessModel.Name),
                BusinessAlreadyExistsMessage);
        }

        var business = new Business
        {
            Name = createBusinessModel.Name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
        };

        // TODO: Add organization support.
        var ownerUsername = await context.Users.Where(u => u.Id == ownerId).Select(u => u.Name).FirstAsync(ct);

        context.Businesses.Add(business);
        await context.SaveChangesAsync(ct);

        return new BusinessDto(ownerUsername, business.Name, business.CreatedAt);
    }

    public async Task<Result<BusinessDto>> GetBusinessAsync(GetBusinessDto getBusinessModel, CancellationToken ct = default)
    {
        var validationResult = await getBusinessModelValidator.ValidateAsync(getBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var queryResult = await context.Businesses.Select(b => new
        {
            BusinessName = b.Name,
            OwnerName = b.Owner.Name,
            b.OwnerId,
            b.CreatedAt,
        }).FirstOrDefaultAsync(x => x.OwnerName == getBusinessModel.Owner && x.BusinessName == getBusinessModel.Name, ct);

        if (queryResult is null)
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        var userPermissions = await businessAuthorizationService.GetPermissionsAsync(queryResult.OwnerName, queryResult.BusinessName, ct);

        // TODO: Add organization support.
        if (queryResult.OwnerId != userResolver.GetUserId() && !userPermissions.Contains(BusinessPermissionEnum.ViewBusiness))
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        return new BusinessDto(queryResult.OwnerName, queryResult.BusinessName, queryResult.CreatedAt);
    }

    public async Task<Result> DeleteBusinessAsync(DeleteBusinessDto deleteBusinessModel, CancellationToken ct = default)
    {
        var validationResult = await deleteBusinessModelValidator.ValidateAsync(deleteBusinessModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.Select(b => new
        {
            b.Id,
            b.Name,
            b.Owner,
        }).FirstOrDefaultAsync(x => x.Owner.Name == deleteBusinessModel.Owner && x.Name == deleteBusinessModel.Business, ct);

        if (business is null)
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        switch (business.Owner)
        {
            case User:
                if (business.Owner.Id != userResolver.GetUserId())
                {
                    return new NotFoundError(BusinessNotFoundMessage);
                }

                break;
            case Organization:
                var userOrganizationPermissions = await organizationAuthorizationService.GetPermissionsAsync(business.Owner.Name, business.Name, ct);
                if (business.Owner.Id != userResolver.GetUserId())
                {
                    if (!userOrganizationPermissions.Contains(OrganizationPermissionEnum.ViewOrganization))
                    {
                        return new NotFoundError(BusinessNotFoundMessage);
                    }

                    if (!userOrganizationPermissions.Contains(OrganizationPermissionEnum.ManageBusinesses))
                    {
                        return new ForbiddenError();
                    }
                }

                break;
        }

        var numberOfDeletedRecords = await context.Businesses.Where(b => b.Id == business.Id).ExecuteDeleteAsync(ct);

        return numberOfDeletedRecords > 0 ? Result.FromSuccess() : new InternalServerError();
    }

    public async Task<Result<BusinessRoleDto>> CreateRoleAsync(CreateBusinessRoleDto createRoleModel, CancellationToken ct = default)
    {
        var validationResult = await createRoleModelValidator.ValidateAsync(createRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.Select(b => new
        {
            b.Id,
            BusinessRoles = b.BusinessRoles.Select(r => r.Name),
            b.Name,
            Owner = new
            {
               Id = b.OwnerId,
               b.Owner.Name,
            },
        }).FirstOrDefaultAsync(
            x => x.Owner.Name == createRoleModel.Owner && x.Name == createRoleModel.Business, cancellationToken: ct);

        if (business is null)
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        var userPermissions = await businessAuthorizationService.GetPermissionsAsync(business.Owner.Name, business.Name, ct);

        // TODO: Add organization support.
        if (business.Owner.Id != userResolver.GetUserId())
        {
            if (!userPermissions.Contains(BusinessPermissionEnum.ViewBusiness))
            {
                return new NotFoundError(BusinessNotFoundMessage);
            }

            if (!userPermissions.Contains(BusinessPermissionEnum.ManageRoles))
            {
                return new ForbiddenError();
            }
        }

        if (business.BusinessRoles.Contains(createRoleModel.Name))
        {
            return new ArgumentConflictError(nameof(createRoleModel.Name), BusinessRoleAlreadyExistsMessage);
        }

        var permissions = new List<BusinessRolePermission>();

        if (createRoleModel.Permissions is not null)
        {
            permissions = createRoleModel.Permissions.Select(p => new BusinessRolePermission
            {
                BusinessPermissionId = (int)FastEnum.Parse<BusinessPermissionEnum>(p),
            }).ToList();
        }

        var role = new BusinessRole
        {
            Name = createRoleModel.Name,
            BusinessId = business.Id,
            BusinessRolePermissions = permissions,
        };

        context.BusinessRoles.Add(role);
        var numberOfWrittenRecords = await context.SaveChangesAsync(ct);

        return numberOfWrittenRecords > 0
            ? new BusinessRoleDto(role.Id, role.Name, createRoleModel.Permissions ?? Array.Empty<string>())
            : new InternalServerError();
    }

    public async Task<Result<IEnumerable<BusinessRoleDto>>> GetAllRolesAsync(GetAllBusinessRolesDto getAllRolesModel, CancellationToken ct = default)
    {
        var validationResult = await getAllRolesModelValidator.ValidateAsync(getAllRolesModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var business = await context.Businesses.Select(b => new
        {
            b.Name,
            b.Owner,
            BusinessRoles = b.BusinessRoles.Select(br => new
            {
                br.Id,
                br.Name,
                Permissions = br.BusinessRolePermissions.Select(brp => brp.BusinessPermission.Name),
            }),
        }).FirstOrDefaultAsync(b => b.Owner.Name == getAllRolesModel.Owner && b.Name == getAllRolesModel.BusinessName, cancellationToken: ct);

        if (business is null)
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        var userPermissions = await businessAuthorizationService.GetPermissionsAsync(business.Owner.Name, business.Name, ct);

        // TODO: Add organization support.
        if (business.Owner.Id != userResolver.GetUserId())
        {
            if (!userPermissions.Contains(BusinessPermissionEnum.ViewBusiness))
            {
                return new NotFoundError(BusinessNotFoundMessage);
            }

            if (!userPermissions.Contains(BusinessPermissionEnum.ManageRoles))
            {
                return new ForbiddenError();
            }
        }

        return business.BusinessRoles.Select(br => new BusinessRoleDto(br.Id, br.Name, br.Permissions)).ToList();
    }

    public async Task<Result<BusinessRoleDto>> UpdateRoleAsync(
        UpdateBusinessRoleDto updateRoleModel,
        CancellationToken ct = default)
    {
        var validationResult = await updateRoleModelValidator.ValidateAsync(updateRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var queryResult = await context.Businesses.AsTracking().Select(b => new
        {
            BusinessName = b.Name,
            BusinessOwnerId = b.Owner.Id,
            BusinessOwnerName = b.Owner.Name,
            BusinessRole = b.BusinessRoles.FirstOrDefault(br => br.Id == updateRoleModel.RoleId),
        }).FirstOrDefaultAsync(b => b.BusinessOwnerName == updateRoleModel.Owner && b.BusinessName == updateRoleModel.Name, ct);

        if (queryResult is null)
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        var userPermissions = await businessAuthorizationService.GetPermissionsAsync(queryResult.BusinessOwnerName, queryResult.BusinessName, ct);

        // TODO: Add organization support.
        if (queryResult.BusinessOwnerId != userResolver.GetUserId())
        {
            if (!userPermissions.Contains(BusinessPermissionEnum.ViewBusiness))
            {
                return new NotFoundError(BusinessNotFoundMessage);
            }

            if (!userPermissions.Contains(BusinessPermissionEnum.ManageRoles))
            {
                return new ForbiddenError();
            }
        }

        if (queryResult.BusinessRole is null)
        {
            return new NotFoundError(BusinessRoleNotFoundMessage);
        }

        if (updateRoleModel.Name is not null)
        {
            if (await context.BusinessRoles.AnyAsync(br => br.Name == updateRoleModel.Name, cancellationToken: ct))
            {
                return new ArgumentConflictError(nameof(updateRoleModel.Name), BusinessRoleAlreadyExistsMessage);
            }

            queryResult.BusinessRole.Name = updateRoleModel.Name;
        }

        if (updateRoleModel.Permissions is not null)
        {
            queryResult.BusinessRole.BusinessRolePermissions.Clear();
            queryResult.BusinessRole.BusinessRolePermissions = updateRoleModel.Permissions.Distinct().Select(p => new BusinessRolePermission
            {
                BusinessPermissionId = (int)FastEnum.Parse<BusinessPermissionEnum>(p),
            }).ToList();
        }

        await context.SaveChangesAsync(ct);

        return new BusinessRoleDto(
            queryResult.BusinessRole.Id,
            queryResult.BusinessRole.Name,
            updateRoleModel.Permissions is not null ? updateRoleModel.Permissions.Distinct() : queryResult.BusinessRole.BusinessRolePermissions.Select(rp => rp.BusinessPermission.Name));
    }

    public async Task<Result> DeleteRoleAsync(DeleteBusinessRoleDto deleteRoleModel, CancellationToken ct = default)
    {
        var validationResult = await deleteRoleModelValidator.ValidateAsync(deleteRoleModel, ct);
        if (!validationResult.IsValid)
        {
            return new AggregateError<ArgumentInvalidError>(
                validationResult.Errors
                    .Select(e => new ArgumentInvalidError(e.PropertyName, e.ErrorMessage))
                    .ToList());
        }

        var queryResult = await context.BusinessRoles.Select(br => new
        {
            br.Id,
            Business = new
            {
                br.Business.Name,
                Owner = new
                {
                    Id = br.Business.OwnerId,
                    br.Business.Owner.Name,
                },
            },
        }).FirstOrDefaultAsync(
            br =>
            br.Business.Owner.Name == deleteRoleModel.Owner &&
            br.Business.Name == deleteRoleModel.Business &&
            br.Id == deleteRoleModel.RoleId,
            cancellationToken: ct);

        if (queryResult is null)
        {
            return new NotFoundError(BusinessNotFoundMessage);
        }

        var userPermissions = await businessAuthorizationService.GetPermissionsAsync(queryResult.Business.Owner.Name, queryResult.Business.Name, ct);

        if (queryResult.Business.Owner.Id != userResolver.GetUserId())
        {
            if (!userPermissions.Contains(BusinessPermissionEnum.ViewBusiness))
            {
                return new NotFoundError(BusinessNotFoundMessage);
            }

            if (!userPermissions.Contains(BusinessPermissionEnum.ManageRoles))
            {
                return new ForbiddenError();
            }
        }

        var numberOfDeletedRecords =
            await context.BusinessRoles.Where(br => br.Id == queryResult.Id).ExecuteDeleteAsync(ct);

        return numberOfDeletedRecords > 0 ? Result.FromSuccess() : new InternalServerError();
    }
}
