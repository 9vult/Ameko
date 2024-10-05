// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;

namespace AssCS;

/// <summary>
/// An event in a subtitle document
/// </summary>
/// <param name="id">ID of the event</param>
public class Event(int id) : BindableBase
{
    private readonly int _id = id;
    private bool _isComment = false;
    private int _layer = 0;
    private Time _start = Time.FromSeconds(0);
    private Time _end = Time.FromSeconds(5);
    private string _style = "Default";
    private string _actor = string.Empty;
    private Margins _margins = new(20, 20, 20);
    private string _effect = string.Empty;
    private string _text = string.Empty;
    private int _index;

    /// <summary>
    /// Event ID
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// If the event is a comment
    /// </summary>
    public bool IsComment
    {
        get => _isComment;
        set => SetProperty(ref _isComment, value);
    }

    /// <summary>
    /// Z-Layer of the event
    /// </summary>
    public int Layer
    {
        get => _layer;
        set => SetProperty(ref _layer, value);
    }

    /// <summary>
    /// Time the event appears on the screen
    /// </summary>
    public Time Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }

    /// <summary>
    /// Time the event ends at
    /// </summary>
    public Time End
    {
        get => _end;
        set => SetProperty(ref _end, value);
    }

    /// <summary>
    /// Name of the style applied to this event
    /// </summary>
    public string Style
    {
        get => _style;
        set
        {
            if (value is not null)
                SetProperty(ref _style, value);
        }
    }

    /// <summary>
    /// Name of the event's actor
    /// </summary>
    public string Actor
    {
        get => _actor;
        set => SetProperty(ref _actor, value);
    }

    /// <summary>
    /// Margins
    /// </summary>
    public Margins Margins
    {
        get => _margins;
        set => SetProperty(ref _margins, value);
    }

    /// <summary>
    /// Effect data
    /// </summary>
    public string Effect
    {
        get => _effect;
        set => SetProperty(ref _effect, value);
    }

    /// <summary>
    /// Text content of the event
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            SetProperty(ref _text, value);
            RaisePropertyChanged(nameof(Cps));
            RaisePropertyChanged(nameof(MaxLineWidth));
        }
    }

    /// <summary>
    /// Row number in the document
    /// </summary>
    public int Index
    {
        get => _index;
        set => _index = value;
    }

    /// <summary>
    /// IDs of extradata linked to this event
    /// </summary>
    public List<int> LinkedExtradatas { get; set; } = [];

    /// <summary>
    /// Characters per second
    /// </summary>
    /// <remarks>It is recommended to keep dialogue events below 18 CPS.</remarks>
    public double Cps
    {
        get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Length (in characters) of the longest line in the event
    /// </summary>
    public int MaxLineWidth
    {
        get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Make inline Lua code snippets ass-compliant
    /// by encoding newlines and indentation to comments
    /// </summary>
    /// <returns>Ass-compliant Lua code</returns>
    /// <remarks>This function reverses <see cref="TransformAssToCode"/></remarks>
    public string TransformCodeToAss()
    {
        var pattern = @"(\r\n|\r|\n)([\ |\t]*)";
        return Regex.Replace(
            Text,
            pattern,
            m =>
            {
                int spacesCount = m.Groups[2].Value.Length;
                return $"--[[{spacesCount}]]";
            }
        );
    }

    /// <summary>
    /// Make inline Lua code snippets display
    /// across multiple lines by decoding
    /// newlines and indentation from comments
    /// </summary>
    /// <returns>Formatted Lua code</returns>
    /// <remarks>This function reverses <see cref="TransformCodeToAss"/></remarks>
    public string TransformAssToCode()
    {
        var pattern = @"--\[\[([0-9]+)\]\]";
        return Regex.Replace(
            Text,
            pattern,
            m =>
            {
                int spacesCount = int.Parse(m.Groups[1].Value);
                return Environment.NewLine + new string(' ', spacesCount);
            }
        );
    }

    /// <summary>
    /// Get the ass representation of this event
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsAss()
    {
        string extradatas =
            LinkedExtradatas.Count > 0 ? $"{{{string.Join("=", LinkedExtradatas)}}}" : "";
        var textContent = Effect.Contains("code") ? TransformCodeToAss() : Text;

        return $"{(IsComment ? "Comment" : "Dialogue")}: {Layer},{Start.AsAss()},{End.AsAss()},{Style},{Actor},"
            + $"{Margins.Left},{Margins.Right},{Margins.Vertical},{Effect},{extradatas}"
            + $"{textContent}";
    }

    /// <summary>
    /// Initialize an event from an ass-formatted string
    /// </summary>
    /// <param name="id">Id of the event to create</param>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Event object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Event FromAss(int id, string data)
    {
        var eventRegex =
            @"^(Comment|Dialogue):\ (\d+),(\d+:\d+:\d+.\d+),(\d+:\d+:\d+.\d+),([^,]*),([^,]*),(-?\d+),(-?\d+),(-?\d+),([^,]*),(.*)";
        var match = Regex.Match(data, eventRegex);
        if (!match.Success)
            throw new ArgumentException($"Event {data} is invalid or malformed.");

        return new Event(id)
        {
            _isComment = match.Groups[1].Value == "Comment",
            _layer = Convert.ToInt32(match.Groups[2].Value),
            _start = Time.FromAss(match.Groups[3].Value),
            _end = Time.FromAss(match.Groups[4].Value),
            _style = match.Groups[5].Value,
            _actor = match.Groups[6].Value,
            _margins =
            {
                Left = Convert.ToInt32(match.Groups[7].Value),
                Right = Convert.ToInt32(match.Groups[8].Value),
                Vertical = Convert.ToInt32(match.Groups[9].Value),
            },
            _effect = match.Groups[10].Value,
            _text = match.Groups[11].Value,
            // TODO: Load extradata
        };
    }

    /// <summary>
    /// Clone this event
    /// </summary>
    /// <returns>Clone of the event</returns>
    /// <remarks>
    /// This method currently uses serialization
    /// as the clone method. This is subject to change.
    /// </remarks>
    public Event Clone()
    {
        return FromAss(Id, AsAss());
    }

    public override bool Equals(object? obj)
    {
        return obj is Event @event
            && _id == @event._id
            && _isComment == @event._isComment
            && _layer == @event._layer
            && _start == @event._start
            && _end == @event._end
            && _style == @event._style
            && _actor == @event._actor
            && _margins == @event._margins
            && _effect == @event._effect
            && _text == @event._text;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_id);
        hash.Add(_isComment);
        hash.Add(_layer);
        hash.Add(_start);
        hash.Add(_end);
        hash.Add(_style);
        hash.Add(_actor);
        hash.Add(_margins);
        hash.Add(_effect);
        hash.Add(_text);
        return hash.ToHashCode();
    }
}
