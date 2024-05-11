using Avalonia.Controls;
using Avalonia.Input;
using Holo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.Services
{
    public class KeybindService
    {
        public static void TrySetKeybind(Control control, KeybindContext context, string key, ICommand command, object? param = null)
        {
            var map = context switch
            {
                KeybindContext.GLOBAL => HoloContext.Instance.ConfigurationManager.KeybindsRegistry.GlobalBinds,
                KeybindContext.GRID => HoloContext.Instance.ConfigurationManager.KeybindsRegistry.GridBinds,
                KeybindContext.EDIT => HoloContext.Instance.ConfigurationManager.KeybindsRegistry.EditBinds,
                KeybindContext.AUDIO => HoloContext.Instance.ConfigurationManager.KeybindsRegistry.AudioBinds,
                KeybindContext.VIDEO => HoloContext.Instance.ConfigurationManager.KeybindsRegistry.VideoBinds,
                _ => new Dictionary<string, string>()
            };
            try
            {
                if (!map.TryGetValue(key, out string? value)) return;
                var gesture = KeyGesture.Parse(value);

                var binding = new KeyBinding { Gesture = gesture, Command = command };
                if (param != null) binding.CommandParameter = param;

                control.KeyBindings.Add(binding);
            }
            catch (Exception ex)
            {
                HoloContext.Logger.Error(ex.Message, "KeybindService");
            }
            
        }
    }
}
