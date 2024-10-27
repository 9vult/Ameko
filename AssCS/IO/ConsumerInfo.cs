// SPDX-License-Identifier: MPL-2.0

namespace AssCS.IO;

/// <summary>
/// Information about the application consuming AssCS
/// </summary>
/// <param name="name">Name of the application</param>
/// <param name="version">Version of the application</param>
/// <param name="website">The application's website</param>
public struct ConsumerInfo(string name, string version, string website)
{
    /// <summary>
    /// Name of the consumer
    /// </summary>
    public readonly string Name => name;

    /// <summary>
    /// Version of the consumer
    /// </summary>
    public readonly string Version => version;

    /// <summary>
    /// The consumer's website
    /// </summary>
    public readonly string Website => website;
}
