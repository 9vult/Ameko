// SPDX-License-Identifier: MPL-2.0

using System.Drawing;
using AssCS.Overrides.Blocks;

namespace AssCS.Overrides;

public class OverrideParameter(VariableType type, ParameterType classification)
{
    private OverrideBlock? _block;
    private readonly VariableType _type = type;
    private readonly ParameterType _classification = classification;
    private bool _isOmitted = true;
    private string _value = string.Empty;

    /// <summary>
    /// Type of variable in the parameter
    /// </summary>
    public VariableType Type => _type;

    /// <summary>
    /// Type of parameter
    /// </summary>
    public ParameterType Classification => _classification;

    /// <summary>
    /// If the parameter is omitted
    /// </summary>
    public bool IsOmitted => _isOmitted;

    /// <summary>
    /// Get the value as a string
    /// </summary>
    /// <returns>String value</returns>
    /// <exception cref="InvalidOperationException">Parameter is omitted</exception>
    public string GetString()
    {
        if (_isOmitted)
        {
            throw new InvalidOperationException(
                "OverrideParameter: Get() called on omitted parameter!"
            );
        }
        if (_block is not null)
        {
            var blockText = _block.Text;
            if (blockText.StartsWith('{'))
                blockText = blockText[1..];
            if (blockText.EndsWith('}'))
                blockText = blockText[..^1];
            return blockText;
        }
        return _value ?? string.Empty;
    }

    /// <summary>
    /// Get the value as an integer
    /// </summary>
    /// <returns>Integer value</returns>
    public int GetInt()
    {
        if (Classification == ParameterType.Alpha)
        {
            var strippedHex = new string(_value.Where(Uri.IsHexDigit).ToArray());
            return Math.Clamp(Convert.ToInt32(strippedHex, 16), 0, 255);
        }
        return Convert.ToInt32(GetString());
    }

    /// <summary>
    /// Get the value as a double
    /// </summary>
    /// <returns>Double value</returns>
    public double GetDouble()
    {
        return Convert.ToDouble(_value);
    }

    /// <summary>
    /// Get the value as a float
    /// </summary>
    /// <returns>Float value</returns>
    public float GetFloat()
    {
        return Convert.ToSingle(_value);
    }

    /// <summary>
    /// Get the value as a boolean
    /// </summary>
    /// <returns>Boolean value</returns>
    public bool GetBool()
    {
        return GetInt() != 0;
    }

    /// <summary>
    /// Get the value as a color
    /// </summary>
    /// <returns>Color value</returns>
    public Color GetColor()
    {
        return Color.FromAss(GetString());
    }

    /// <summary>
    /// Get the value as an override block
    /// </summary>
    /// <returns>Override block value</returns>
    public OverrideBlock GetOverrideBlock()
    {
        if (_block == null)
        {
            _block = new OverrideBlock(GetString());
            _block.ParseTags();
        }
        return _block;
    }

    /// <summary>
    /// Set the value to a new string
    /// </summary>
    /// <param name="newValue">New value</param>
    public void Set(string newValue)
    {
        _isOmitted = false;
        _value = newValue;
        _block = null;
    }

    /// <summary>
    /// Set the value to a new integer
    /// </summary>
    /// <param name="newValue">New value</param>
    public void Set(int newValue)
    {
        if (Classification == ParameterType.Alpha)
            Set($"&H{Math.Clamp(newValue, 0, 255):X2}&");
        else
            Set(Convert.ToString(newValue));
    }

    /// <summary>
    /// Set the value to a new double
    /// </summary>
    /// <param name="newValue">New value</param>
    public void Set(double newValue)
    {
        Set(Convert.ToString(newValue));
    }

    /// <summary>
    /// Set the value to a new boolean
    /// </summary>
    /// <param name="newValue">New value</param>
    public void Set(bool newValue)
    {
        Set(newValue ? 1 : 0);
    }
}
