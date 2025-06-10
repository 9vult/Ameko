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

    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<Dictionary<string, OverridePrototype>> _prototypes = new(() =>
    {
        var dict = new Dictionary<string, OverridePrototype>();
        LoadPrototypes(dict);
        return dict;
    });

    public static readonly Dictionary<string, OverridePrototype> Prototypes = _prototypes.Value;

    private static void LoadPrototypes(Dictionary<string, OverridePrototype> prototypes)
    {
        if (prototypes.Count > 0)
            return;

        prototypes.Add(
            @"\alpha",
            new OverridePrototype(@"\alpha", VariableType.Text, ParameterType.Alpha)
        );
        prototypes.Add(
            @"\bord",
            new OverridePrototype(@"\bord", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\xbord",
            new OverridePrototype(@"\xbord", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\ybord",
            new OverridePrototype(@"\ybord", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\shad",
            new OverridePrototype(@"\shad", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\xshad",
            new OverridePrototype(@"\xshad", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\yshad",
            new OverridePrototype(@"\yshad", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\fade",
            new OverridePrototype(
                @"\fade",
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
        prototypes.Add(
            @"\move",
            new OverridePrototype(
                @"\move",
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
        prototypes.Add(
            @"\clip4",
            new OverridePrototype(
                @"\clip",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                ]
            )
        );
        prototypes.Add(
            @"\clip2",
            new OverridePrototype(
                @"\clip",
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
        prototypes.Add(
            @"\iclip4",
            new OverridePrototype(
                @"\iclip",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                ]
            )
        );
        prototypes.Add(
            @"\iclip2",
            new OverridePrototype(
                @"\iclip",
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
        prototypes.Add(
            @"\fscx",
            new OverridePrototype(@"\fscx", VariableType.Float, ParameterType.RelativeSizeX)
        );
        prototypes.Add(
            @"\fscy",
            new OverridePrototype(@"\fscy", VariableType.Float, ParameterType.RelativeSizeY)
        );
        prototypes.Add(
            @"\pos",
            new OverridePrototype(
                @"\pos",
                [
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Float, ParameterType.AbsolutePosY),
                ]
            )
        );
        prototypes.Add(
            @"\org",
            new OverridePrototype(
                @"\org",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosX),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.AbsolutePosY),
                ]
            )
        );
        prototypes.Add(
            @"\pbo",
            new OverridePrototype(@"\pbo", VariableType.Int, ParameterType.AbsolutePosY)
        );
        prototypes.Add(
            @"\fad",
            new OverridePrototype(
                @"\fad",
                [
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeStart),
                    new OverridePrototypeParam(VariableType.Int, ParameterType.RelativeTimeEnd),
                ]
            )
        );
        prototypes.Add(
            @"\fsp",
            new OverridePrototype(@"\fsp", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(@"\frx", new OverridePrototype(@"\frx", VariableType.Float));
        prototypes.Add(@"\fry", new OverridePrototype(@"\fry", VariableType.Float));
        prototypes.Add(@"\frz", new OverridePrototype(@"\frz", VariableType.Float));
        prototypes.Add(@"\fr", new OverridePrototype(@"\fr", VariableType.Float));
        prototypes.Add(@"\fax", new OverridePrototype(@"\fax", VariableType.Float));
        prototypes.Add(@"\fay", new OverridePrototype(@"\fay", VariableType.Float));
        prototypes.Add(
            @"\1c",
            new OverridePrototype(@"\1c", VariableType.Text, ParameterType.Color)
        );
        prototypes.Add(
            @"\2c",
            new OverridePrototype(@"\2c", VariableType.Text, ParameterType.Color)
        );
        prototypes.Add(
            @"\3c",
            new OverridePrototype(@"\3c", VariableType.Text, ParameterType.Color)
        );
        prototypes.Add(
            @"\4c",
            new OverridePrototype(@"\4c", VariableType.Text, ParameterType.Color)
        );
        prototypes.Add(
            @"\1a",
            new OverridePrototype(@"\1a", VariableType.Text, ParameterType.Alpha)
        );
        prototypes.Add(
            @"\2a",
            new OverridePrototype(@"\2a", VariableType.Text, ParameterType.Alpha)
        );
        prototypes.Add(
            @"\3a",
            new OverridePrototype(@"\3a", VariableType.Text, ParameterType.Alpha)
        );
        prototypes.Add(
            @"\4a",
            new OverridePrototype(@"\4a", VariableType.Text, ParameterType.Alpha)
        );
        prototypes.Add(@"\fe", new OverridePrototype(@"\fe", VariableType.Text));
        prototypes.Add(
            @"\ko",
            new OverridePrototype(@"\ko", VariableType.Int, ParameterType.Karaoke)
        );
        prototypes.Add(
            @"\kf",
            new OverridePrototype(@"\kf", VariableType.Int, ParameterType.Karaoke)
        );
        prototypes.Add(
            @"\be",
            new OverridePrototype(@"\be", VariableType.Int, ParameterType.AbsoluteSize)
        );
        prototypes.Add(
            @"\blur",
            new OverridePrototype(@"\blur", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(@"\fn", new OverridePrototype(@"\fn", VariableType.Text));
        prototypes.Add(@"\fs+", new OverridePrototype(@"\fs+", VariableType.Float));
        prototypes.Add(@"\fs-", new OverridePrototype(@"\fs-", VariableType.Float));
        prototypes.Add(
            @"\fs",
            new OverridePrototype(@"\fs", VariableType.Float, ParameterType.AbsoluteSize)
        );
        prototypes.Add(@"\an", new OverridePrototype(@"\an", VariableType.Int));
        prototypes.Add(@"\c", new OverridePrototype(@"\c", VariableType.Text, ParameterType.Color));
        prototypes.Add(@"\b", new OverridePrototype(@"\b", VariableType.Int));
        prototypes.Add(@"\i", new OverridePrototype(@"\i", VariableType.Bool));
        prototypes.Add(@"\u", new OverridePrototype(@"\u", VariableType.Bool));
        prototypes.Add(@"\s", new OverridePrototype(@"\s", VariableType.Bool));
        prototypes.Add(@"\a", new OverridePrototype(@"\a", VariableType.Int));
        prototypes.Add(
            @"\k",
            new OverridePrototype(@"\k", VariableType.Int, ParameterType.Karaoke)
        );
        prototypes.Add(
            @"\K",
            new OverridePrototype(@"\K", VariableType.Int, ParameterType.Karaoke)
        );
        prototypes.Add(@"\q", new OverridePrototype(@"\q", VariableType.Int));
        prototypes.Add(@"\p", new OverridePrototype(@"\p", VariableType.Int));
        prototypes.Add(@"\r", new OverridePrototype(@"\r", VariableType.Int));
        prototypes.Add(
            @"\t",
            new OverridePrototype(
                @"\t",
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
