namespace Rogue.Contacts.Service.Cryptography.Interfaces;

public interface IHashService
{
    Task<string> ComputeHashAsync(string text);

    Task<CompareResult> CompareHashAsync(string text, string correctTextHash);
}