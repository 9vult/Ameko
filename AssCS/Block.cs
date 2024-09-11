using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssCS
{
    /// <summary>
    /// Representation of a single segment of an Event.
    /// </summary>
    public abstract class Block : IAssComponent
    {
        protected string _text;
        public BlockType Type { get; }
        public Block(string data)
        {
            _text = data;
        }

        protected Block(string data, BlockType type)
        {
            _text = data;
            Type = type;
        }

        public virtual string Text
        {
            get => _text;
        }

        public string AsAss() => _text;
        public string? AsOverride() => _text;
    }

    /// <summary>
    /// Any text in an event that is not a comment, override, or drawing
    /// should be considered to be in a Plaintext Block.
    /// </summary>
    public class PlainBlock : Block
    {
        public PlainBlock(string data) :
            base(
                data,
                BlockType.PLAIN
                )
        { }
    }

    /// <summary>
    /// Text within a set of override brackets <code>{ }</code>
    /// that does not contain an override are Comments.
    /// </summary>
    public class CommentBlock : Block
    {
        public CommentBlock(string data) :
            base(
                $"{{{data}}}",
                BlockType.COMMENT
                )
        { }
    }

    /// <summary>
    /// Text following any drawing level greater than 0 is assumed
    /// to be a Drawing. A drawing is initiated by a preceeding Override
    /// Block containg the drawing override P tag:
    /// <code>{\p[drawinglevel]}</code>
    /// </summary>
    public class DrawingBlock : Block
    {
        public int Scale { get; set; }
        public DrawingBlock(string data, int scale) :
            base(
                data,
                BlockType.DRAWING
                )
        { 
            Scale = scale;
        }
    }

    /// <summary>
    /// An Override block is composed of a set of override brackets
    /// containing one or more Override Tags
    /// </summary>
    public class OverrideBlock : Block
    {
        public List<OverrideTag> Tags { get; }

        public OverrideBlock(string data) : base(data, BlockType.OVERRIDE)
        {
            Tags = new List<OverrideTag>();
        }

        /// <summary>
        /// Parse the tags in the override block
        /// </summary>
        public void ParseTags()
        {
            Tags.Clear();
            var depth = 0;
            var start = 0;
            for (int i = 1; i < _text.Length; ++i)
            {
                if (depth > 0)
                {
                    if (_text[i] == ')') --depth;

                }
                else if (_text[i] == '\\')
                {
                    Tags.Add(new OverrideTag(_text.Substring(start, i - start)));
                    start = i;
                }
                else if (_text[i] == '(') ++depth;
            }
            if (_text.Length > 0) Tags.Add(new OverrideTag(_text[start..]));
        }

        /// <summary>
        /// Add a tag to the block
        /// </summary>
        /// <param name="tag"></param>
        public void AddTag(string tag)
        {
            Tags.Add(new OverrideTag(tag));
        }

        public override string Text
        {
            get
            {
                _text = $"{{{string.Join("", Tags.Select(t => t.ToString()))}}}"; ;
                return _text;
            }
        }
    }

    /// <summary>
    /// Type of Block
    /// </summary>
    public enum BlockType
    {
        PLAIN,
        COMMENT,
        OVERRIDE,
        DRAWING
    }
}
