// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.Text;

namespace AssCS.IO;

/// <summary>
/// Supports the reading and parsing of subtitle files
/// </summary>
public abstract class FileParser
{
    /// <summary>
    /// Parse the contents of a <see cref="TextReader"/> into a <see cref="Document"/>
    /// </summary>
    /// <param name="reader">Data to parse</param>
    /// <returns><see cref="Document"/> represented by the <paramref name="reader"/></returns>
    protected abstract Document Parse(TextReader reader);

    /// <summary>
    /// Parse a file into a <see cref="Document"/>
    /// </summary>
    /// <param name="savePath">Path to the file to open</param>
    /// <returns><see cref="Document"/> at the <paramref name="savePath"/></returns>
    public Document Parse(Uri savePath)
    {
        return Parse(new FileSystem(), savePath);
    }

    /// <summary>
    /// Parse a file into a <see cref="Document"/>
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Path to the file to open</param>
    /// <returns><see cref="Document"/> at the <paramref name="savePath"/></returns>
    public Document Parse(IFileSystem fileSystem, Uri savePath)
    {
        var path = savePath.LocalPath;

        if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
            fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

        using var fs = fileSystem.FileStream.New(path, FileMode.OpenOrCreate);
        using var reader = new StreamReader(fs, encoding: Encoding.UTF8);

        return Parse(reader);
    }
}
