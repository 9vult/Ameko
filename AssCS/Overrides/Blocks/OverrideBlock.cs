// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides.Blocks;

/// <summary>
/// An override block
/// </summary>
/// <remarks>
/// An override block is composed of a set
/// of override brackets <c>{ }</c> containing
/// one or more override tags
/// </remarks>
public class OverrideBlock(string data) : Block(data, BlockType.Override)
{
    private readonly List<OverrideTag> _tags = [];

    /// <summary>
    /// Override tags in this block
    /// </summary>
    public List<OverrideTag> Tags => _tags;

    public override string Text
    {
        get
        {
            _text = $"{{{string.Join("", Tags.Select(t => t.ToString()))}}}";
            ;
            return _text;
        }
    }

    /// <summary>
    /// Parse the tags within the block
    /// </summary>
    public void ParseTags()
    {
        _tags.Clear();
        var depth = 0;
        var start = 0;
        for (int i = 1; i < _text.Length; ++i)
        {
            if (depth > 0)
            {
                if (_text[i] == ')')
                    --depth;
            }
            else if (_text[i] == '\\')
            {
                _tags.Add(new OverrideTag(_text[start..i]));
                start = i;
            }
            else if (_text[i] == '(')
                ++depth;
        }
        if (_text.Length > 0)
            _tags.Add(new OverrideTag(_text[start..]));
    }

    /// <summary>
    /// Add a tag to the block
    /// </summary>
    /// <param name="tag">Tag to add</param>
    public void AddTag(string tag)
    {
        _tags.Add(new OverrideTag(tag));
    }
}
