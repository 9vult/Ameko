using AssCS;
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
    public class LogGridErrorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;
            LogLevel level = (LogLevel)value;

            if (level == LogLevel.ERROR)
            {
                return new SolidColorBrush(Colors.Red);
            }
            return Brushes.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
