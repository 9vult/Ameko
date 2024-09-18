using AssCS;
using Ffms2CS;
using Holo.DC;
using Holo.Plugins;
using LibassCS;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Xdg.Directories;

namespace Holo
{
    public class HoloContext
    {
        private static readonly Lazy<HoloContext> _instance = new Lazy<HoloContext>(() => new HoloContext());
        
        public static HoloContext Instance => _instance.Value;

        public static Logger Logger { get; } = Logger.Instance;
        public PluginHandler PluginHandler { get; }
        public ConfigurationManager ConfigurationManager { get; }
        public GlobalsManager GlobalsManager { get; }
        public RepositoryManager RepositoryManager { get; }

        public Workspace Workspace { get; set; }

        private HoloContext()
        {
            Directories.Create();
            LoggerHelper.Initialize();

            ConfigurationManager = new ConfigurationManager(Directories.HoloConfigHome);
            GlobalsManager = new GlobalsManager(Directories.HoloDataHome);
            PluginHandler = new PluginHandler(Directories.HoloDataHome);

            RepositoryManager = new RepositoryManager();
            RepositoryManager.LoadUrlList(ConfigurationManager.GetRepositories());

            Workspace = new Workspace();
        }

        public class Directories
        {
            public static string HoloDataHome => Path.Combine(BaseDirectory.DataHome, "Ameko", "Holo");
            public static string HoloConfigHome => Path.Combine(BaseDirectory.ConfigHome, "Ameko", "Holo");
            public static string HoloCacheHome => Path.Combine(BaseDirectory.CacheHome, "Ameko", "Holo");
            public static string HoloStateHome => Path.Combine(BaseDirectory.StateHome, "Ameko", "Holo");
            public static string AmekoDataHome => Path.Combine(BaseDirectory.DataHome, "Ameko");
            public static string AmekoConfigHome => Path.Combine(BaseDirectory.ConfigHome, "Ameko");
            public static string AmekoCacheHome => Path.Combine(BaseDirectory.CacheHome, "Ameko");
            public static string AmekoStateHome => Path.Combine(BaseDirectory.StateHome, "Ameko");

            public static void Create()
            {
                if (!Directory.Exists(AmekoDataHome)) Directory.CreateDirectory(AmekoDataHome);
                if (!Directory.Exists(AmekoConfigHome)) Directory.CreateDirectory(AmekoConfigHome);
                if (!Directory.Exists(AmekoCacheHome)) Directory.CreateDirectory(AmekoCacheHome);
                if (!Directory.Exists(AmekoStateHome)) Directory.CreateDirectory(AmekoStateHome);
                if (!Directory.Exists(HoloDataHome)) Directory.CreateDirectory(HoloDataHome);
                if (!Directory.Exists(HoloConfigHome)) Directory.CreateDirectory(HoloConfigHome);
                if (!Directory.Exists(HoloCacheHome)) Directory.CreateDirectory(HoloCacheHome);
                if (!Directory.Exists(HoloStateHome)) Directory.CreateDirectory(HoloStateHome);
            }
        }
    }
}
