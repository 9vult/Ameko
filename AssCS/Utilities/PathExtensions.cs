// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Utilities;

public static class PathExtensions
{
    private const char ForwardSlash = '/';

    /// <summary>
    /// Returns a relative path from one path to another using forward slashes
    /// </summary>
    /// <param name="relativeTo">The source path the result should be relative to</param>
    /// <param name="path">The destination path</param>
    /// <returns>The relative path, or path if the paths don't share the same root</returns>
    public static string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path)
            .Replace(Path.DirectorySeparatorChar, ForwardSlash);
    }
}
