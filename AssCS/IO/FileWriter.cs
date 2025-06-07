// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
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
    protected abstract bool Write(TextWriter writer, bool export = false);

    /// <summary>
    /// Write an ass document to file
    /// </summary>
    /// <param name="savePath">Path to write to</param>
    /// <param name="export"><see langword="true"/> if this write is an export</param>
    /// <returns><see langword="true"/> if writing was successful</returns>
    /// <exception cref="IOException">If writing fails</exception>
    public bool Write(Uri savePath, bool export = false)
    {
        return Write(new FileSystem(), savePath, export);
    }

    /// <summary>
    /// Write an ass document to file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Path to write to</param>
    /// <param name="export"><see langword="true"/> if this write is an export</param>
    /// <returns><see langword="true"/> if writing was successful</returns>
    /// <exception cref="IOException">If writing fails</exception>
    public bool Write(IFileSystem fileSystem, Uri savePath, bool export = false)
    {
        var path = savePath.LocalPath;

        if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
            fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

        using var fs = fileSystem.FileStream.New(path, FileMode.OpenOrCreate);
        using var writer = new StreamWriter(fs, encoding: Encoding.UTF8);

        var result = Write(writer, export);

        writer.Flush();
        fs.SetLength(fs.Position);
        return result;
    }
}
