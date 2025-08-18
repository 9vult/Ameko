// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

public class GitStatusEntry(string filePath, bool isStaged)
{
    public string FilePath => filePath;
    public string FileName => Path.GetFileName(filePath);
    public bool IsStaged => isStaged;
}
