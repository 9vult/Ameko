// SPDX-License-Identifier: MPL-2.0

using Holo.Scripting.Models;

namespace Holo.Scripting;

public interface IHoloExecutable
{
    /// <summary>
    /// Basic script information
    /// </summary>
    public PackageInfo Info { get; }
}
