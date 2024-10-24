// SPDX-License-Identifier: MPL-2.0

using System.Text;

namespace AssCS.IO;

/// <summary>
/// Supports the writing of subtitle files
/// </summary>
public abstract class FileWriter
{
    /// <summary>
    /// Write an ass document to a TextWriter
    /// </summary>
    /// <param name="writer">Writer to write to</param>
    /// <param name="export"><see langword="true"/> if this write is an export</param>
    /// <returns><see langword="true"/> if writing was successful</returns>
    public abstract bool Write(TextWriter writer, bool export = false);

    /// <summary>
    /// Write an ass document to file
    /// </summary>
    /// <param name="filename">Path to write to</param>
    /// <param name="export"><see langword="true"/> if this write is an export</param>
    /// <returns><see langword="true"/> if writing was successful</returns>
    public bool Write(string filename, bool export = false)
    {
        using var file = File.Open(filename, FileMode.OpenOrCreate);
        using var writer = new StreamWriter(file, encoding: Encoding.UTF8);
        return Write(writer, export);
    }
}
