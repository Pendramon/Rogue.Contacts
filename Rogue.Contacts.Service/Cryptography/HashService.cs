using Rogue.Contacts.Service.Cryptography.Interfaces;

namespace Rogue.Contacts.Service.Cryptography;

public sealed class HashService : IHashService
{
    /// <summary>
    /// All of the hashing functions used in the lifetime of the application including the current one.
    /// </summary>
    private readonly IEnumerable<IHashFunction> hashFunctions;

    /// <summary>
    /// The current AKA the latest hashing function that is being used,
    /// that all old hashes should be rehashed with.
    /// </summary>
    private readonly IHashFunction currentHashFunction;

    public HashService(IEnumerable<IHashFunction> hashFunctions)
    {
        this.hashFunctions = hashFunctions;
        this.currentHashFunction = this.hashFunctions.First(x => x is BCryptFunction);
    }

    /// <summary>
    /// Computes a hash value using the current hashing function.
    /// </summary>
    /// <param name="text">The text for the input data.</param>
    /// <returns>The hashed value with the salt and settings concatenated.</returns>
    public async Task<string> ComputeHashAsync(string text)
    {
        return await this.currentHashFunction.ComputeHashAsync(text);
    }

    /// <summary>
    /// Hashes the text with a corresponding hashing function based on the hash format
    /// and compares the text's hash with the given hash to check if it matches.
    /// If the two hashes match and the hashing function used to generate the given hash
    /// isn't the current hashing function, it will rehash the text with the proper hashing
    /// function and its settings.
    /// </summary>
    /// <param name="text">The text to hash and compare.</param>
    /// <param name="hash">The correct text hash to match against.</param>
    /// <returns>The <see cref="CompareResult"/> of the comparison.</returns>
    public async Task<CompareResult> CompareHashAsync(string text, string hash)
    {
        var hashingFunction = this.hashFunctions.SingleOrDefault(hashFunction => hashFunction.IsValidHash(hash));

        _ = hashingFunction ?? throw new ArgumentException(
            "The hashing function used to generate the hash isn't recognized by the application.");

        var result = await hashingFunction.CompareHashAsync(text, hash);

        if (hashingFunction.GetType() == this.currentHashFunction.GetType())
        {
            return result;
        }

        if (result.Match)
        {
            result.RehashResult = await this.currentHashFunction.ComputeHashAsync(text);
        }

        return result;
    }
}