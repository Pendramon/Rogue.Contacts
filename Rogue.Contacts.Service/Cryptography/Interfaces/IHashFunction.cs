namespace Rogue.Contacts.Service.Cryptography.Interfaces;

public interface IHashFunction
{
    Task<string> ComputeHashAsync(string text);

    Task<CompareResult> CompareHashAsync(string text, string correctTextHash);

    bool IsValidHash(string hash);
}