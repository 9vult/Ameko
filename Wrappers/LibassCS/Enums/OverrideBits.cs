namespace LibassCS.Enums
{
    [Flags]
    public enum OverrideBits
    {
        Default = 0,
        Style = 1 << 0,
        SlectiveFontScale = 1 << 1,
        [Obsolete("Replaced by SelectiveFontScale")]
        FontSize = 1 << 1,
        FontSizeFields = 1 << 2,
        FontName = 1 << 3,
        Colors = 1 << 4,
        Attributes = 1 << 5,
        Border = 1 << 6,
        Alignment = 1 << 7,
        FullStyle = 1 << 8,
        Justify = 1 << 9
    }
}
