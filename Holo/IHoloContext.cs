// SPDX-License-Identifier: MPL-2.0

using Holo.Providers;

namespace Holo;

public interface IHoloContext
{
    IConfiguration Config { get; }
    Globals Globals { get; }
    ISolutionProvider SolutionProvider { get; }
}
