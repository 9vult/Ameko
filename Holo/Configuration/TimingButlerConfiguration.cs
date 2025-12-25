// SPDX-License-Identifier: MPL-2.0

using AssCS;

namespace Holo.Configuration;

public class TimingButlerConfiguration : BindableBase
{
    public uint LeadIn { get; set; }
    public uint LeadOut { get; set; }
    public uint SnapStartEarlierThreshold { get; set; }
    public uint SnapStartLaterThreshold { get; set; }
    public uint SnapEndEarlierThreshold { get; set; }
    public uint SnapEndLaterThreshold { get; set; }
    public uint ChainThreshold { get; set; }
}
