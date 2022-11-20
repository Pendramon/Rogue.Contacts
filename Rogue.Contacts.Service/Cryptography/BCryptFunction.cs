using Rogue.Contacts.Service.Cryptography.Interfaces;
using static BCrypt.Net.BCrypt;

namespace Rogue.Contacts.Service.Cryptography;

public sealed class BCryptFunction : IHashFunction
{
    private const byte WorkFactor = 12;

    /// <summary>
    /// Computes the hash value for the specified string after the value is pre-hashed with SHA384.
    /// </summary>
    /// <param name="text">The text for the input data.</param>
    /// <returns>The hashed value with the salt and options concatenated.</returns>
    public async Task<string> ComputeHashAsync(string text)
    {
        // BCrypt library does not support async hashing hence this workaround.
        var hash = await Task.Run(() => EnhancedHashPassword(text, WorkFactor));
        return hash;
    }

    /// <summary>
    /// Compares two hashes to see if the given text's hash matches the provided hash.
    /// If the hash was using a different work factor than the current work factor it
    /// will compute a new hash accordingly with the current work factor.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <param name="hash">The correct text hash.</param>
    /// <returns>The <see cref="CompareResult"/> of the comparison.</returns>
    public async Task<CompareResult> CompareHashAsync(string text, string hash)
    {
        var result = new CompareResult();
        var match = EnhancedVerify(text, hash);
        result.Match = match;
        if (result.Match && PasswordNeedsRehash(hash, WorkFactor))
        {
            var newHash = await this.ComputeHashAsync(text);
            result.RehashResult = newHash;
        }

        return result;
    }

    /// <summary>
    /// Checks whether the given hash is in valid format as the format supported by this hashing function.
    /// </summary>
    /// <param name="hash">The hash to validate.</param>
    /// <returns><see langword="true"/> if the hash is in valid format, otherwise <see langword="false"/>.</returns>
    public bool IsValidHash(string hash)
    {
        if (hash.Length != 59 && hash.Length != 60)
        {
            // Incorrect full hash length
            return false;
        }

        if (!hash.StartsWith("$2"))
        {
            // Not a bcrypt hash
            return false;
        }

        // Validate version
        var offset = 2;
        if (IsValidBCryptVersionChar(hash[offset]))
        {
            offset++;
        }

        if (hash[offset++] != '$')
        {
            return false;
        }

        // Validate workfactor
        if (!IsAsciiNumeric(hash[offset++])
            || !IsAsciiNumeric(hash[offset++]))
        {
            return false;
        }

        if (hash[offset++] != '$')
        {
            return false;
        }

        // Validate hash
        for (var i = offset; i < hash.Length; ++i)
        {
            if (!IsValidBCryptBase64Char(hash[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidBCryptVersionChar(char value)
    {
        return value
            is 'a'
            or 'b'
            or 'x'
            or 'y';
    }

    private static bool IsValidBCryptBase64Char(char value)
    {
        // Ordered by ascending ASCII value
        return value
            is '.'
            or '/'
            or >= '0' and <= '9'
            or >= 'A' and <= 'Z'
            or >= 'a' and <= 'z';
    }

    private static bool IsAsciiNumeric(char value)
    {
        return value is >= '0' and <= '9';
    }
}