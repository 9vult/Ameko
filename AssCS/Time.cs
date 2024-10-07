// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

/// <summary>
/// A timestamp
/// </summary>
/// <remarks>
/// This API supports millisecond percision, but note that
/// precision will be dropped to centiseconds for export.
/// </remarks>
public class Time : BindableBase, IComparable<Time>
{
    private TimeSpan _local;

    /// <summary>
    /// Total number of hours
    /// </summary>
    public long Hours => _local.Days * 24 + _local.Hours;

    /// <summary>
    /// Number of minutes
    /// </summary>
    public long Minutes => _local.Minutes;

    /// <summary>
    /// Number of seconds
    /// </summary>
    public long Seconds => _local.Seconds;

    /// <summary>
    /// Tenths of a second
    /// </summary>
    public long Deciseconds => _local.Milliseconds / 100;

    /// <summary>
    /// Hundredths of a second
    /// </summary>
    /// <remarks>
    /// Centiseconds are the maximum precision
    /// supported by exported files
    /// </remarks>
    public long Centiseconds => _local.Milliseconds / 10;

    /// <summary>
    /// Thousandths of a second
    /// </summary>
    public long Milliseconds => _local.Milliseconds;

    /// <summary>
    /// Total number of seconds
    /// </summary>
    public double TotalSeconds => _local.TotalSeconds;

    /// <summary>
    /// Total number of milliseconds
    /// </summary>
    public long TotalMilliseconds => (long)_local.TotalMilliseconds;

    /// <summary>
    /// Ass-formatted string
    /// </summary>
    public string TextContent => AsAss();

    /// <summary>
    /// Time-formatted string with milliseconds
    /// </summary>
    public string MillisecondText => $"{Hours}:{Minutes:00}:{Seconds:00}.{Milliseconds:000}";

    /// <summary>
    /// Updatable text for GUIs
    /// </summary>
    /// <remarks>
    /// <para>
    /// This field is intended to be used as a two-way binding.
    /// GET returns an ass-formatted string, and SET takes in
    /// an ass-formatted string.
    /// </para>
    /// <para>
    /// The caller must take responsibility for ensuring
    /// that inputs are valid timestamps.
    /// </para>
    /// </remarks>
    public string UpdatableText
    {
        get => AsAss();
        set
        {
            var splits = value.Split(':', '.');
            if (splits.Length != 4)
                throw new ArgumentException($"Time: {value} is an invalid timecode.");
            long millis = 0;
            int[] multiplier = [3600 * 1000, 60 * 1000, 1000, 10];
            for (int i = 0; i < splits.Length; i++)
            {
                millis += Convert.ToInt64(splits[i]) * multiplier[i];
            }
            _local = TimeSpan.FromMilliseconds(millis);
            RaisePropertyChanged(nameof(UpdatableText));
        }
    }

    /// <summary>
    /// Get the ass representation of this time
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsAss()
    {
        return $"{Hours}:{Minutes:00}:{Seconds:00}.{Centiseconds:00}";
    }

    /// <summary>
    /// Initialize a Time object from an ass-formatted Time string
    /// </summary>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Time object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Time FromAss(string data)
    {
        var splits = data.Split(':', '.');
        if (splits.Length != 4)
            throw new ArgumentException($"Time: {data} is an invalid timecode.");
        long millis = 0;
        int[] multiplier = [3600 * 1000, 60 * 1000, 1000, 10];
        for (int i = 0; i < splits.Length; i++)
        {
            millis += Convert.ToInt64(splits[i]) * multiplier[i];
        }
        return FromMillis(millis);
    }

    /// <summary>
    /// Create a Time object from milliseconds
    /// </summary>
    /// <param name="millis">Number of milliseconds</param>
    /// <returns>Time object</returns>
    public static Time FromMillis(long millis) => new(TimeSpan.FromMilliseconds(millis));

    /// <summary>
    /// Create a Time object from centiseconds
    /// </summary>
    /// <param name="centis">Number of centiseconds</param>
    /// <returns>Time object</returns>
    public static Time FromCentis(long centis) => new(TimeSpan.FromMilliseconds(centis * 10));

    /// <summary>
    /// Create a Time object from deciseconds
    /// </summary>
    /// <param name="decis">Number of deciseconds</param>
    /// <returns>Time object</returns>
    public static Time FromDecis(long decis) => new(TimeSpan.FromMilliseconds(decis * 100));

    /// <summary>
    /// Create a Time object from seconds
    /// </summary>
    /// <param name="secs">Number of seconds</param>
    /// <returns>Time object</returns>
    public static Time FromSeconds(double secs) => new(TimeSpan.FromSeconds(secs));

    /// <summary>
    /// Create a Time object from another Time
    /// </summary>
    /// <param name="t">Time object</param>
    /// <returns>Time object</returns>
    public static Time FromTime(Time t) => new(t);

    /// <summary>
    /// Initialize a zero time
    /// </summary>
    public Time()
    {
        _local = TimeSpan.Zero;
    }

    /// <summary>
    /// Initialize a time from a TimeSpan
    /// </summary>
    /// <param name="t">TimeSpan</param>
    public Time(TimeSpan t)
    {
        _local = t;
    }

    /// <summary>
    /// Initialize a time from another Time
    /// </summary>
    /// <param name="t">Time object</param>
    public Time(Time t)
    {
        _local = TimeSpan.FromMilliseconds(t.TotalMilliseconds);
    }

    public int CompareTo(Time? other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return obj is Time time && _local.TotalMilliseconds.Equals(time._local.TotalMilliseconds);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_local.TotalMilliseconds);
    }

    #region Operators

    public static bool operator ==(Time? left, Time? right)
    {
        return left?._local.Milliseconds == right?._local.Milliseconds;
    }

    public static bool operator !=(Time? left, Time? right)
    {
        return !(left == right);
    }

    public static Time operator +(Time a, Time b)
    {
        return new Time(a._local + b._local);
    }

    public static Time operator -(Time a, Time b)
    {
        var temp = new Time(a._local - b._local);
        if (temp.TotalMilliseconds < 0)
            temp = new Time();
        return temp;
    }

    public static bool operator >(Time a, Time b)
    {
        return a._local > b._local;
    }

    public static bool operator <(Time a, Time b)
    {
        return a._local < b._local;
    }

    public static bool operator >=(Time a, Time b)
    {
        return a._local >= b._local;
    }

    public static bool operator <=(Time a, Time b)
    {
        return a._local <= b._local;
    }

    #endregion
}
