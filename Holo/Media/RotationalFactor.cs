// SPDX-License-Identifier: MPL-2.0

namespace Holo.Media;

/// <summary>
/// Represents the rotational factor of a video viewport
/// </summary>
public class RotationalFactor
{
    /// <summary>
    /// Textual representation of the rotation
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Angle of the rotation
    /// </summary>
    public required double Angle { get; init; }

    /// <summary>
    /// Default scale
    /// </summary>
    public static RotationalFactor Default => new() { Text = "0%", Angle = 0d };

    /// <summary>
    /// Valid scale factors
    /// </summary>
    public static List<RotationalFactor> Angles =>
        Enumerable
            .Range(0, 16) // 22.5 * 16 = 360
            .Select(i =>
            {
                var scale = i * 22.5;
                return new RotationalFactor { Text = $"{scale}°", Angle = scale };
            })
            .ToList();

    protected bool Equals(RotationalFactor other)
    {
        return Angle.Equals(other.Angle);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((RotationalFactor)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Angle.GetHashCode();
    }
}
