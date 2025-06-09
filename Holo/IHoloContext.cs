// SPDX-License-Identifier: MPL-2.0

using AssCS.Utilities;
using Holo.Providers;
using Holo.Scripting;

namespace Holo;

public interface IHoloContext
{
    Configuration Configuration { get; }
    Globals Globals { get; }
    ISolutionProvider SolutionProvider { get; }
}
