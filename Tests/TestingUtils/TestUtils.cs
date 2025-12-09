// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;

namespace TestingUtils;

public static class TestableUri
{
    /// <summary>
    /// Create a cross-platform <see cref="Uri"/> from a relative <paramref name="path"/>
    /// </summary>
    /// <param name="fs">FileSystem to use</param>
    /// <param name="path">Relative path</param>
    /// <returns>Cross-platform absolute path</returns>
    public static Uri MakeTestableUri(MockFileSystem fs, string path)
    {
        return new Uri(fs.Path.Combine(fs.Directory.GetCurrentDirectory(), path));
    }
}
