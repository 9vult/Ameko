using Ameko.DataModels;
using Ameko.Services;
using AssCS;
using Avalonia.Threading;
using DynamicData;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class ScriptPropertiesWindowViewModel : ViewModelBase
    {
        public List<ScriptPropertyWrapper> Properties { get; }

        public ScriptPropertiesWindowViewModel(File file)
        {
            Properties = file.InfoManager.GetAll().Select(kv => new ScriptPropertyWrapper(kv.Key, kv.Value, file)).ToList();
        }

        public class ScriptPropertyWrapper
        {
            private readonly string _key;
            private string _value;
            private File _file;

            public string Key => _key;

            public string Value
            {
                get => _value;
                set
                {
                    _value = value;
                    _file.InfoManager.Set(Key, value);
                }
            }

            public ScriptPropertyWrapper(string key, string value, File file)
            {
                _key = key;
                _value = value;
                _file = file;
            }
        }

    }
}
