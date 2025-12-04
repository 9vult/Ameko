// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.Services;

public interface IMemoryService
{
    uint CurrentAllocation { get; }
}
