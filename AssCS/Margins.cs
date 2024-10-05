// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

/// <summary>
/// Margins within the video frame
/// </summary>
/// <param name="left">Left margin</param>
/// <param name="right">Right margin</param>
/// <param name="vertical">Top and bottom margin</param>
public class Margins(int left, int right, int vertical) : BindableBase
{
    private int _left = left;
    private int _right = right;
    private int _vertical = vertical;

    /// <summary>
    /// Left margin
    /// </summary>
    public int Left
    {
        get => _left;
        set => SetProperty(ref _left, value);
    }

    /// <summary>
    /// Right margin
    /// </summary>
    public int Right
    {
        get => _right;
        set => SetProperty(ref _right, value);
    }

    /// <summary>
    /// Top and bottom margin
    /// </summary>
    public int Vertical
    {
        get => _vertical;
        set => SetProperty(ref _vertical, value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Margins margins
            && _left == margins._left
            && _right == margins._right
            && _vertical == margins._vertical;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_left, _right, _vertical);
    }
}
