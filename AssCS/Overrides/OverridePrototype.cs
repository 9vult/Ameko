// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides;

public class OverridePrototype
{
    public string Name { get; }
    public List<OverridePrototypeParam> Parameters { get; }

    public void AddParam(
        VariableType type,
        ParameterType classification = ParameterType.Normal,
        Optional opt = Optional.NotOptional
    )
    {
        Parameters.Add(new OverridePrototypeParam(type, classification, opt));
    }

    public OverridePrototype(string name, List<OverridePrototypeParam> paramList)
    {
        Name = name;
        Parameters = paramList;
    }

    public OverridePrototype(
        string name,
        VariableType type,
        ParameterType classification = ParameterType.Normal,
        Optional opt = Optional.NotOptional
    )
    {
        Name = name;
        Parameters = [];
        AddParam(type, classification, opt);
    }

    public static readonly Dictionary<string, OverridePrototype> Prototypes = [];

    public static void LoadPrototypes()
    {
        if (Prototypes.Count > 0)
            return;

        Prototypes.Add(
            "\\alpha",
            new OverridePrototype("\\alpha", VariableType.Text, ParameterType.Alpha)
        );
        Prototypes.Add(
            "\\bord",
            new OverridePrototype("\\bord", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\xbord",
            new OverridePrototype("\\xbord", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\ybord",
            new OverridePrototype("\\ybord", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\shad",
            new OverridePrototype("\\shad", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\xshad",
            new OverridePrototype("\\xshad", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\yshad",
            new OverridePrototype("\\yshad", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\fade",
            new OverridePrototype(
                "\\fade",
                [
                    new OverridePrototypeParam(VariableType.Int),
                    new OverridePrototypeParam(VariableType.Int),
                    new OverridePrototypeParam(VariableType.Int),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                ]
            )
        );
        Prototypes.Add(
            "\\move",
            new OverridePrototype(
                "\\move",
                [
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosY),
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosY),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                ]
            )
        );
        Prototypes.Add(
            "\\clip4",
            new OverridePrototype(
                "\\clip",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                ]
            )
        );
        Prototypes.Add(
            "\\clip2",
            new OverridePrototype(
                "\\clip",
                [
                    new OverridePrototypeParam(
                        VariableType.Int,
                        ParameterType.Normal,
                        Optional.Optional2
                    ),
                    new OverridePrototypeParam(VariableType.Text, ParameterType.Drawing),
                ]
            )
        );
        Prototypes.Add(
            "\\iclip4",
            new OverridePrototype(
                "\\iclip",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                ]
            )
        );
        Prototypes.Add(
            "\\iclip2",
            new OverridePrototype(
                "\\iclip",
                [
                    new OverridePrototypeParam(
                        VariableType.Int,
                        ParameterType.Normal,
                        Optional.Optional2
                    ),
                    new OverridePrototypeParam(VariableType.Text, ParameterType.Drawing),
                ]
            )
        );
        Prototypes.Add(
            "\\fscx",
            new OverridePrototype("\\fscx", VariableType.Float, ParameterType.RelativeSizeX)
        );
        Prototypes.Add(
            "\\fscy",
            new OverridePrototype("\\fscy", VariableType.Float, ParameterType.RelativeSizeY)
        );
        Prototypes.Add(
            "\\pos",
            new OverridePrototype(
                "\\pos",
                [
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosY),
                ]
            )
        );
        Prototypes.Add(
            "\\org",
            new OverridePrototype(
                "\\org",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                ]
            )
        );
        Prototypes.Add(
            "\\pbo",
            new OverridePrototype("\\pbo", VariableType.Int, ParameterType.AbsolutePosY)
        );
        Prototypes.Add(
            "\\fad",
            new OverridePrototype(
                "\\fad",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeEnd),
                ]
            )
        );
        Prototypes.Add(
            "\\fsp",
            new OverridePrototype("\\fsp", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add("\\frx", new OverridePrototype("\\frx", VariableType.Float));
        Prototypes.Add("\\fry", new OverridePrototype("\\fry", VariableType.Float));
        Prototypes.Add("\\frz", new OverridePrototype("\\frz", VariableType.Float));
        Prototypes.Add("\\fr", new OverridePrototype("\\fr", VariableType.Float));
        Prototypes.Add("\\fax", new OverridePrototype("\\fax", VariableType.Float));
        Prototypes.Add("\\fay", new OverridePrototype("\\fay", VariableType.Float));
        Prototypes.Add(
            "\\1c",
            new OverridePrototype("\\1c", VariableType.Text, ParameterType.Color)
        );
        Prototypes.Add(
            "\\2c",
            new OverridePrototype("\\2c", VariableType.Text, ParameterType.Color)
        );
        Prototypes.Add(
            "\\3c",
            new OverridePrototype("\\3c", VariableType.Text, ParameterType.Color)
        );
        Prototypes.Add(
            "\\4c",
            new OverridePrototype("\\4c", VariableType.Text, ParameterType.Color)
        );
        Prototypes.Add(
            "\\1a",
            new OverridePrototype("\\1a", VariableType.Text, ParameterType.Alpha)
        );
        Prototypes.Add(
            "\\2a",
            new OverridePrototype("\\2a", VariableType.Text, ParameterType.Alpha)
        );
        Prototypes.Add(
            "\\3a",
            new OverridePrototype("\\3a", VariableType.Text, ParameterType.Alpha)
        );
        Prototypes.Add(
            "\\4a",
            new OverridePrototype("\\4a", VariableType.Text, ParameterType.Alpha)
        );
        Prototypes.Add("\\fe", new OverridePrototype("\\fe", VariableType.Text));
        Prototypes.Add(
            "\\ko",
            new OverridePrototype("\\ko", VariableType.Int, ParameterType.Karaoke)
        );
        Prototypes.Add(
            "\\kf",
            new OverridePrototype("\\kf", VariableType.Int, ParameterType.Karaoke)
        );
        Prototypes.Add(
            "\\be",
            new OverridePrototype("\\be", VariableType.Int, ParameterType.AbsoluteSize)
        );
        Prototypes.Add(
            "\\blur",
            new OverridePrototype("\\blur", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add("\\fn", new OverridePrototype("\\fn", VariableType.Text));
        Prototypes.Add("\\fs+", new OverridePrototype("\\fs+", VariableType.Float));
        Prototypes.Add("\\fs-", new OverridePrototype("\\fs-", VariableType.Float));
        Prototypes.Add(
            "\\fs",
            new OverridePrototype("\\fs", VariableType.Float, ParameterType.AbsoluteSize)
        );
        Prototypes.Add("\\an", new OverridePrototype("\\an", VariableType.Int));
        Prototypes.Add("\\c", new OverridePrototype("\\c", VariableType.Text, ParameterType.Color));
        Prototypes.Add("\\b", new OverridePrototype("\\b", VariableType.Int));
        Prototypes.Add("\\i", new OverridePrototype("\\i", VariableType.Bool));
        Prototypes.Add("\\u", new OverridePrototype("\\u", VariableType.Bool));
        Prototypes.Add("\\s", new OverridePrototype("\\s", VariableType.Bool));
        Prototypes.Add("\\a", new OverridePrototype("\\a", VariableType.Int));
        Prototypes.Add(
            "\\k",
            new OverridePrototype("\\k", VariableType.Int, ParameterType.Karaoke)
        );
        Prototypes.Add(
            "\\K",
            new OverridePrototype("\\K", VariableType.Int, ParameterType.Karaoke)
        );
        Prototypes.Add("\\q", new OverridePrototype("\\q", VariableType.Int));
        Prototypes.Add("\\p", new OverridePrototype("\\p", VariableType.Int));
        Prototypes.Add("\\r", new OverridePrototype("\\r", VariableType.Int));
        Prototypes.Add(
            "\\t",
            new OverridePrototype(
                "\\t",
                [
                    new OverridePrototypeParam(
                        VariableType.Int,
                        ParameterType.RelativeTimeStart,
                        Optional.Optional3 | Optional.Optional4
                    ),
                    new OverridePrototypeParam(
                        VariableType.Int,
                        ParameterType.RelativeTimeStart,
                        Optional.Optional3 | Optional.Optional4
                    ),
                    new OverridePrototypeParam(
                        VariableType.Float,
                        ParameterType.Normal,
                        Optional.Optional2 | Optional.Optional4
                    ),
                    new OverridePrototypeParam(VariableType.Block),
                ]
            )
        );
    }
}

public class OverridePrototypeParam(
    VariableType type,
    ParameterType classification = ParameterType.Normal,
    Optional optional = Optional.NotOptional
)
{
    public Optional Optional { get; } = optional;
    public VariableType Type { get; } = type;
    public ParameterType Classification { get; } = classification;
}

[Flags]
public enum Optional : uint
{
    NotOptional = 0xFF,
    Optional1 = 0x01,
    Optional2 = 0x02,
    Optional3 = 0x04,
    Optional4 = 0x08,
    Optional5 = 0x10,
    Optional6 = 0x20,
    Optional7 = 0x40,
}
