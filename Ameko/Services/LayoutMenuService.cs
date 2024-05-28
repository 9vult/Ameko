using Avalonia.Controls;
using Avalonia.Svg.Skia;
using Holo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.Services
{
    public class LayoutMenuService
    {
        public static List<MenuItem> GenerateLayoutMenuItemSource(ICommand UseLayoutCommand)
        {
            List<MenuItem> congregation = [];

            var layouts = LayoutService.Instance.Layouts;

            foreach (var layout in layouts )
            {
                if (layout.Name == null) continue;
                var svg = new Avalonia.Svg.Skia.Svg(new Uri("avares://Ameko/Assets/B5/columns-gap.svg")) { Path = new Uri("avares://Ameko/Assets/B5/columns-gap.svg").LocalPath };

                var mi = new MenuItem
                {
                    Header = layout.Name,
                    Command = UseLayoutCommand,
                    CommandParameter = layout.Name,
                    Icon = svg
                };

                congregation.Add(mi);
            }

            return congregation;
        }
    }
}
