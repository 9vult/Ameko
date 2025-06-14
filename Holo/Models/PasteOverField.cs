// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

[Flags]
public enum PasteOverField
{
    None = 0,
    Comment = 1,
    Layer = 2,
    StartTime = 4,
    EndTime = 8,
    Style = 16,
    Actor = 32,
    MarginLeft = 64,
    MarginRight = 128,
    MarginVertical = 256,
    Effect = 512,
    Text = 1024,
}
