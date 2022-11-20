namespace Rogue.Contacts.Service.Cryptography;

public class CompareResult
{
    /// <summary>
    /// Gets or sets a value indicating whether both of the hashes match.
    /// </summary>
    public bool Match { get; set; }

    /// <summary>
    /// Gets a value indicating whether the hash got rehashed due to the hash
    /// not matching the current iteration count or hashing function.
    /// </summary>
    public bool Rehashed => this.RehashResult != null;

    /// <summary>
    /// Gets or sets the new hash that has been rehashed due to the hash
    /// not matching the current iteration count or hashing function.
    /// Use <see cref="Rehashed"/> to figure out if the hash has been rehashed.
    /// </summary>
    public string? RehashResult { get; set; }
}