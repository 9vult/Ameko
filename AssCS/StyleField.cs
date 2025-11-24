// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

[Flags]
public enum StyleField
{
    None = 0,
    Name = 1 << 0,
    FontFamily = 1 << 1,
    FontSize = 1 << 2,
    PrimaryColor = 1 << 3,
    SecondaryColor = 1 << 4,
    OutlineColor = 1 << 5,
    ShadowColor = 1 << 6,
    IsBold = 1 << 7,
    IsItalic = 1 << 8,
    IsUnderline = 1 << 9,
    IsStrikethrough = 1 << 10,
    ScaleX = 1 << 11,
    ScaleY = 1 << 12,
    Spacing = 1 << 13,
    Angle = 1 << 14,
    BorderStyle = 1 << 15,
    BorderThickness = 1 << 16,
    ShadowDistance = 1 << 17,
    Alignment = 1 << 18,
    MarginLeft = 1 << 19,
    MarginRight = 1 << 20,
    MarginVertical = 1 << 21,
    Encoding = 1 << 22,
    All =
        Name
        | FontFamily
        | FontSize
        | PrimaryColor
        | SecondaryColor
        | OutlineColor
        | ShadowColor
        | IsBold
        | IsItalic
        | IsUnderline
        | IsStrikethrough
        | ScaleX
        | ScaleY
        | Spacing
        | Angle
        | BorderStyle
        | BorderThickness
        | ShadowDistance
        | Alignment
        | MarginLeft
        | MarginRight
        | MarginVertical
        | Encoding,
}
