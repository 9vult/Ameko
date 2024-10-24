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
    private readonly int _id = id;
    private int _expiration = expiration;
    private string _key = key;
    private string _value = value;

    /// <summary>
    /// ID of the entry
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// Expiration counter
    /// </summary>
    public int Expiration
    {
        get => _expiration;
        set => _expiration = value;
    }

    /// <summary>
    /// Entry key
    /// </summary>
    public string Key
    {
        get => _key;
        set => _key = value;
    }

    /// <summary>
    /// Base64-encoded value
    /// </summary>
    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public int CompareTo(ExtradataEntry? other)
    {
        return Id.CompareTo(other?.Id);
    }
}
