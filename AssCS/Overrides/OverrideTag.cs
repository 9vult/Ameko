// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides;

public class OverrideTag
{
    private bool _isValid = false;
    private readonly List<OverrideParameter> _parameters = [];
    private string _name;

    public bool IsValid => _isValid;
    public List<OverrideParameter> Parameters => _parameters;
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Clear out the tag
    /// </summary>
    public void Clear()
    {
        _parameters.Clear();
        _isValid = false;
    }

    /// <summary>
    /// Set the tag text
    /// </summary>
    /// <param name="data">Text to set</param>
    public void SetText(string data)
    {
        OverridePrototype.LoadPrototypes();

        foreach (OverridePrototype p in OverridePrototype.Prototypes.Values)
        {
            if (data.StartsWith(p.Name))
            {
                Name = p.Name;
                ParseParameters(data[p.Name.Length..]);
                _isValid = true;
                return;
            }
        }
        // Garbage :poppo:
        Name = data;
        _isValid = false;
    }

    /// <summary>
    /// Parse tag parameters
    /// </summary>
    /// <param name="data">Tag parameters</param>
    public void ParseParameters(string data)
    {
        Clear();
        List<string> paramList = Tokenize(data);
        int parsFlag = 1 << (paramList.Count - 1); // Optional parameters flag

        OverridePrototype p;
        if (Name == "\\clip" && paramList.Count != 4)
            p = OverridePrototype.Prototypes["\\clip2"];
        else if (Name == "\\clip")
            p = OverridePrototype.Prototypes["\\clip4"];
        else if (Name == "\\iclip" && paramList.Count != 4)
            p = OverridePrototype.Prototypes["\\iclip2"];
        else if (Name == "\\iclip")
            p = OverridePrototype.Prototypes["\\iclip4"];
        else
            p = OverridePrototype.Prototypes[Name];

        int curPar = 0;
        foreach (var param in p.Parameters)
        {
            Parameters.Add(new OverrideParameter(param.Type, param.Classification));
            if (((uint)param.Optional & parsFlag) == 0 || curPar >= paramList.Count)
                continue;

            Parameters.Last().Set(paramList[curPar++]);
        }
    }

    /// <summary>
    /// Tokenize parameters
    /// </summary>
    /// <param name="data">Data to tokenize</param>
    /// <returns>List of tokens</returns>
    public static List<string> Tokenize(string data)
    {
        List<string> paramList = [];
        if (data.Length <= 0)
            return paramList;

        if (data[0] != '(')
        {
            // No parentheses → single-parameter override
            paramList.Add(data.Trim());
            return paramList;
        }

        // Parsing time uwu
        var i = 0;
        var parDepth = 1;
        while (i < data.Length && parDepth > 0)
        {
            // Hunt for next comma or parenthesis.
            var start = ++i;
            while (i < data.Length && parDepth > 0)
            {
                char c = data[i];
                if (c == ',' && parDepth == 1)
                    break;
                if (c == '(')
                    parDepth++;
                else if (c == ')')
                {
                    if (--parDepth == 0)
                    {
                        break;
                    }
                }
                i++;
            }
            paramList.Add(data[start..i].Trim());
        }

        if (i + 1 < data.Length)
        {
            paramList.Add(data[(i + 1)..]);
        }
        return paramList;
    }

    /// <summary>
    /// Initialize an empty tag
    /// </summary>
    public OverrideTag()
    {
        _name = string.Empty;
    }

    /// <summary>
    /// Initialize a tag with some data
    /// </summary>
    /// <param name="data">Data to set</param>
    public OverrideTag(string data)
    {
        _name = string.Empty;
        SetText(data);
    }

    /// <summary>
    /// Initialize a tag from another tag
    /// </summary>
    /// <param name="tag">Tag to clone</param>
    public OverrideTag(OverrideTag tag)
    {
        _name = tag._name;
        _parameters = new(tag._parameters);
        _isValid = tag._isValid;
    }

    public override string ToString()
    {
        string result = Name;
        bool parentheses = Parameters.Count > 1;
        if (parentheses)
            result += "(";
        result += string.Join(",", Parameters.Where(p => !p.IsOmitted).Select(p => p.GetString()));
        if (parentheses)
            result += ")";
        return result;
    }
}
