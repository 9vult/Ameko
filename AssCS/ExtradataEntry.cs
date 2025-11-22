// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

/// <summary>
/// A subset implementation of Aegisub Extradata
/// </summary>
/// <param name="id">ID of the entry</param>
/// <param name="expiration">Expiration counter</param>
/// <param name="key">Entry key</param>
/// <param name="value">Base64-encoded value</param>
/// <remarks>
/// While Aegisub's extradata implementation supports inline_string encoding,
/// prefixed with <c>e</c>, and UUEncoding, prefixed with <c>u</c>, AssCS only
/// supports standard Base64 encoding, with the prefix <c>b</c>.
/// </remarks>
public class ExtradataEntry(int id, int expiration, string key, string value)
    : IComparable<ExtradataEntry>
{
    /// <summary>
    /// ID of the entry
    /// </summary>
    public int Id => id;

    /// <summary>
    /// Expiration counter
    /// </summary>
    public int Expiration { get; set; } = expiration;

    /// <summary>
    /// Entry key
    /// </summary>
    public string Key { get; set; } = key;

    /// <summary>
    /// Base64-encoded value
    /// </summary>
    public string Value { get; set; } = value;

    /// <summary>
    /// Clone this extradata entry
    /// </summary>
    /// <returns>Clone of self</returns>
    public ExtradataEntry Clone()
    {
        return new ExtradataEntry(Id, Expiration, Key, Value);
    }

    public int CompareTo(ExtradataEntry? other)
    {
        return Id.CompareTo(other?.Id);
    }
}
