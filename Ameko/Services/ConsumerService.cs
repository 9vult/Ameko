// SPDX-License-Identifier: GPL-3.0-only

using AssCS.IO;

namespace Ameko.Services;

public static class ConsumerService
{
    public static ConsumerInfo AmekoInfo { get; } =
        new("Ameko", VersionService.FullLabel, "https://ameko.moe");
}
