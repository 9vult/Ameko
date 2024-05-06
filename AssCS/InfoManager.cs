using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AssCS
{
    /// <summary>
    /// Manages the script information for a file
    /// </summary>
    public class InfoManager
    {
        private readonly Dictionary<string, string> info;

        public void Set(string key, string value)
        {
            info[key] = value;
        }

        public string? Get(string key)
        {
            if (info.ContainsKey(key)) return info[key];
            return null;
        }

        public bool Remove(string key)
        {
            return info.Remove(key);
        }

        public void Clear()
        {
            info.Clear();
        }

        public Dictionary<string, string> GetAll()
        {
            return info;
        }

        public void LoadDefault()
        {
            Clear();
            Set("Title", "Default File");
            Set("Original Script", "");
            Set("Original Translation", "");
            Set("Original Editing", "");
            Set("Original Timing", "");
            Set("Synch Point", "");
            Set("Script Updated By", "");
            Set("Update Details", "");
            Set("ScriptType", "v4.00+");
            Set("PlayResX", "1920");
            Set("PlayResY", "1080");
            Set("Timer", "0.0000");
            Set("WrapStyle", "0");
            Set("ScaledBorderAndShadow", "yes");
            Set("YCbCr Matrix", "TV.709");
        }

        public InfoManager(File source)
        {
            info = new Dictionary<string, string>(source.InfoManager.info);
        }

        public InfoManager() 
        {
            info = new Dictionary<string, string>();
        }
    }
}
