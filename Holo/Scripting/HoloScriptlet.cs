// SPDX-License-Identifier: MPL-2.0

using Acornima.Ast;
using Holo.Scripting.Models;
using Jint;

namespace Holo.Scripting;

public class HoloScriptlet : IHoloExecutable
{
    public required PackageInfo Info { get; init; }

    public Prepared<Script> CompiledScript { get; init; }
}
