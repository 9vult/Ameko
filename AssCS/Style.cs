using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AssCS
{
    /// <summary>
    /// Represents a style that can be applied to an event
    /// </summary>
    public class Style : IAssComponent, ICommitable, INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _font;
        private double _fontSize;
        private Color _primary;
        private Color _secondary;
        private Color _outline;
        private Color _shadow;
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private bool _strikeout;
        private double _scaleX;
        private double _scaleY;
        private double _spacing;
        private double _angle;
        private int _borderStyle;
        private double _borderThickness;
        private double _shadowDistance;
        private int _alignment;
        private Margins _margins;
        private int _encoding;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        public string Font
        {
            get => _font;
            set { _font = value; OnPropertyChanged(nameof(Font)); }
        }
        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }
        public Color Primary
        {
            get => _primary;
            set { _primary = value; OnPropertyChanged(nameof(Primary)); }
        }
        public Color Secondary
        {
            get => _secondary;
            set { _secondary = value; OnPropertyChanged(nameof(Secondary)); }
        }
        public Color Outline
        {
            get => _outline;
            set { _outline = value; OnPropertyChanged(nameof(Outline)); }
        }
        public Color Shadow
        {
            get => _shadow;
            set { _shadow = value; OnPropertyChanged(nameof(Shadow)); }
        }
        public bool Bold
        {
            get => _bold;
            set { _bold = value; OnPropertyChanged(nameof(Bold)); }
        }
        public bool Italic
        {
            get => _italic;
            set { _italic = value; OnPropertyChanged(nameof(Italic)); }
        }
        public bool Underline
        {
            get => _underline;
            set { _underline = value; OnPropertyChanged(nameof(Underline)); }
        }
        public bool Strikeout
        {
            get => _strikeout;
            set { _strikeout = value; OnPropertyChanged(nameof(Strikeout)); }
        }
        public double ScaleX
        {
            get => _scaleX;
            set { _scaleX = value; OnPropertyChanged(nameof(ScaleX)); }
        }
        public double ScaleY
        {
            get => _scaleY;
            set { _scaleY = value; OnPropertyChanged(nameof(ScaleY)); }
        }
        public double Spacing
        {
            get => _spacing;
            set { _spacing = value; OnPropertyChanged(nameof(Spacing)); }
        }
        public double Angle
        {
            get => _angle;
            set { _angle = value; OnPropertyChanged(nameof(Angle)); }
        }
        public int BorderStyle
        {
            get => _borderStyle;
            set { _borderStyle = value; OnPropertyChanged(nameof(BorderStyle)); }
        }
        public double BorderThickness
        {
            get => _borderThickness;
            set { _borderThickness = value; OnPropertyChanged(nameof(BorderThickness)); }
        }
        public double ShadowDistance
        {
            get => _shadowDistance;
            set { _shadowDistance = value; OnPropertyChanged(nameof(ShadowDistance)); }
        }
        public int Alignment
        {
            get => _alignment;
            set { _alignment = value; OnPropertyChanged(nameof(Alignment)); }
        }
        public Margins Margins
        {
            get => _margins;
            set { _margins = value; OnPropertyChanged(nameof(Margins)); }
        }
        public int Encoding
        {
            get => _encoding;
            set { _encoding = value; OnPropertyChanged(nameof(Encoding)); }
        }

        /// <summary>
        /// Bootstrap this style from its representation in a file
        /// </summary>
        /// <param name="data">Line</param>
        public void FromAss(string data)
        {
            var styleRegex = @"Style:\ ([^,]*),([^,]*),([\d.]+),(&H[\da-fA-F]{8}&?),(&H[\da-fA-F]{8}&?),(&H[\da-fA-F]{8}&?),(&H[\da-fA-F]{8}&?),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+),(-?[\d.]+)";
            var match = Regex.Match(data, styleRegex);
            if (!match.Success) throw new ArgumentException($"Style {data} is invalid or malformed.");

            Name = match.Groups[1].Value;
            Font = match.Groups[2].Value;
            FontSize = Convert.ToDouble(match.Groups[3].Value);
            Primary = new Color(match.Groups[4].Value);
            Secondary = new Color(match.Groups[5].Value);
            Outline = new Color(match.Groups[6].Value);
            Shadow = new Color(match.Groups[7].Value);
            Bold = Convert.ToInt32(match.Groups[8].Value) != 0;
            Italic = Convert.ToInt32(match.Groups[9].Value) != 0;
            Underline = Convert.ToInt32(match.Groups[10].Value) != 0;
            Strikeout = Convert.ToInt32(match.Groups[11].Value) != 0;
            ScaleX = Convert.ToDouble(match.Groups[12].Value);
            ScaleY = Convert.ToDouble(match.Groups[13].Value);
            Spacing = Convert.ToDouble(match.Groups[14].Value);
            Angle = Convert.ToDouble(match.Groups[15].Value);
            BorderStyle = Convert.ToInt32(match.Groups[16].Value);
            BorderThickness = Convert.ToDouble(match.Groups[17].Value);
            ShadowDistance = Convert.ToDouble(match.Groups[18].Value);
            Alignment = Convert.ToInt32(match.Groups[19].Value);
            Margins = new Margins(
                    Convert.ToInt32(match.Groups[20].Value),
                    Convert.ToInt32(match.Groups[21].Value),
                    Convert.ToInt32(match.Groups[22].Value)
                );
            Encoding = Convert.ToInt32(match.Groups[23].Value);
        }

        public string AsAss()
        {
            var cleanName = Name.Replace(',', ';');
            var cleanFont = Font.Replace(',', ';');
            return $"Style: {cleanName},{cleanFont},{FontSize},{Primary.AsAss()},{Secondary.AsAss()},{Outline.AsAss()},{Shadow.AsAss()}," +
                $"{(Bold ? -1 : 0)},{(Italic ? -1 : 0)},{(Underline ? -1 : 0)},{(Strikeout ? -1 : 0)}," +
                $"{ScaleX},{ScaleY},{Spacing},{Angle},{BorderStyle},{BorderThickness},{ShadowDistance},{Alignment}," +
                $"{Margins.Left},{Margins.Right},{Margins.Vertical},{Encoding}";
        }

        public string? AsOverride() => null;

        public Style (int id, Style s)
        {
            _id = id;
            _name = s.Name;
            _font = s.Font;
            _fontSize = s.FontSize;
            _primary = new Color(s.Primary);
            _secondary = new Color(s.Secondary);
            _outline = new Color(s.Outline);
            _shadow = new Color(s.Shadow);
            _bold = s.Bold;
            _italic = s.Italic;
            _underline = s.Underline;
            _strikeout = s.Strikeout;
            _scaleX = s.ScaleX;
            _scaleY = s.ScaleY;
            _spacing = s.Spacing;
            _angle = s.Angle;
            _borderStyle = s.BorderStyle;
            _borderThickness = s.BorderThickness;
            _shadowDistance = s.ShadowDistance;
            _alignment = s.Alignment;
            _margins = new Margins(s.Margins);
            _encoding = s.Encoding;
        }

        public Style(int id)
        {
            _id = id;
            _name = "Default";
            _font = "Arial";
            _fontSize = 48.0;
            _primary = new Color(255, 255, 255);
            _secondary = new Color(255, 0, 0); // Red
            _outline = new Color(0, 0, 0);
            _shadow = new Color(0, 0, 0);
            _bold = false;
            _italic = false;
            _underline = false;
            _strikeout = false;
            _scaleX = 100.0;
            _scaleY = 100.0;
            _spacing = 0.0;
            _angle = 0.0;
            _borderStyle = 1;
            _borderThickness = 2.0;
            _shadowDistance = 2.0;
            _alignment = 2;
            _margins = new Margins(0, 0, 0);
            _encoding = 1;
        }

        public Style(int id, string data) : this(id)
        {
            FromAss(data);
        }

        public Style Clone()
        {
            return new Style(this.Id, this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Margins : INotifyPropertyChanged
    {
        private int left;
        private int right;
        private int vertical;
        public int Left
        {
            get => left;
            set { left = value; OnPropertyChanged(nameof(Left)); }
        }
        public int Right
        {
            get => right;
            set { right = value; OnPropertyChanged(nameof(Right)); }
        }
        public int Vertical
        {
            get => vertical;
            set { vertical = value; OnPropertyChanged(nameof(Vertical)); }
        }

        public Margins(int left, int right, int vertical)
        {
            Left = left;
            Right = right;
            Vertical = vertical;
        }

        public Margins(Margins m)
        {
            Left = m.Left;
            Right = m.Right;
            Vertical = m.Vertical;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!obj.GetType().Equals(typeof(Margins))) return false;
            Margins o = (Margins)obj;
            return Left == o.Left
                && Right == o.Right
                && Vertical == o.Vertical;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Right, Vertical);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
