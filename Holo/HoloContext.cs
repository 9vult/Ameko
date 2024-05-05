﻿using Holo.DC;
using System;
using System.Diagnostics;
using System.IO;
using Xdg.Directories;

namespace Holo
{
    public class HoloContext
    {
        private static readonly Lazy<HoloContext> _instance = new Lazy<HoloContext>(() => new HoloContext());
        
        public static HoloContext Instance => _instance.Value;
        
        public Logger Logger { get; }
        public PluginHandler PluginHandler { get; }
        public ConfigurationManager ConfigurationManager { get; }
        public GlobalsManager GlobalsManager { get; }
        public RepositoryManager RepositoryManager { get; }
        public Workspace Workspace { get; set; }

        private HoloContext()
        {
            Directories.Create();

            Logger = Logger.Instance;
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
            public static string AmekoDataHome => Path.Combine(BaseDirectory.DataHome, "Ameko");
            public static string AmekoConfigHome => Path.Combine(BaseDirectory.ConfigHome, "Ameko");
            public static string AmekoCacheHome => Path.Combine(BaseDirectory.CacheHome, "Ameko");

            public static void Create()
            {
                if (!Directory.Exists(AmekoDataHome)) Directory.CreateDirectory(AmekoDataHome);
                if (!Directory.Exists(AmekoConfigHome)) Directory.CreateDirectory(AmekoConfigHome);
                if (!Directory.Exists(AmekoCacheHome)) Directory.CreateDirectory(AmekoCacheHome);
                if (!Directory.Exists(HoloDataHome)) Directory.CreateDirectory(HoloDataHome);
                if (!Directory.Exists(HoloConfigHome)) Directory.CreateDirectory(HoloConfigHome);
                if (!Directory.Exists(HoloCacheHome)) Directory.CreateDirectory(HoloCacheHome);
            }
        }
    }
}
