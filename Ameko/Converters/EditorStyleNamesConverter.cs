using Ameko.DataModels;
using AssCS;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Holo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ameko.Converters
{
    public class EditorStyleNamesConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var name = (string)value;
            return HoloContext.Instance.Workspace.WorkingFile.File.StyleManager.Get(name);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return ((Style)value).Name;
        }
    }
}
