namespace Rogue.Contacts.View.Model;

public sealed record GetBusinessRequestDto
{
    public GetBusinessRequestDto(string businessId)
    {
        BusinessId = businessId;
    }

    public GetBusinessRequestDto(string ownerUsername, string businessName)
    {
        OwnerUsername = ownerUsername;
        BusinessName = businessName;
    }

    public string? BusinessId { get; }

    public string? OwnerUsername { get; }

    public string? BusinessName { get; }
}
