// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

public class GitBranch(string branchName, bool isRemote, bool isTracking)
{
    public string Name => branchName;
    public bool IsRemote => isRemote;
    public bool IsTracking => isTracking;
}
