using AssCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Holo
{
    public class PluginHandler
    {
        private readonly string pluginRoot;
        private readonly string audioRoot;
        private readonly string videoRoot;
        private readonly string subtitleRoot;

        private readonly Dictionary<string, IAudioSourcePlugin> audioPlugins;
        private readonly Dictionary<string, IVideoSourcePlugin> videoPlugins;
        private readonly Dictionary<string, ISubtitlePlugin> subtitlePlugins;

        public bool LoadAll()
        {
            if (!Directory.Exists(pluginRoot) ||
                !Directory.Exists(audioRoot) ||
                !Directory.Exists(videoRoot) ||
                !Directory.Exists(subtitleRoot)
            ) return false;

            foreach (var path in Directory.EnumerateFiles(audioRoot))
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(path);
                    foreach (Type type in a.GetTypes())
                    {
                        if (typeof(IAudioSourcePlugin).IsAssignableFrom(type))
                            if (Activator.CreateInstance(type) is IAudioSourcePlugin p)
                                audioPlugins.Add(p.Name, p);

                        if (typeof(IVideoSourcePlugin).IsAssignableFrom(type))
                            if (Activator.CreateInstance(type) is IVideoSourcePlugin p)
                                videoPlugins.Add(p.Name, p);

                        if (typeof(ISubtitlePlugin).IsAssignableFrom(type))
                            if (Activator.CreateInstance(type) is ISubtitlePlugin p)
                                subtitlePlugins.Add(p.Name, p);
                    }
                } catch (Exception ex)
                {
                    Logger.Instance.Error(ex.Message, "PluginHandler"); // TOOD
                }
            }
            return true;
        }

        public bool UnloadAll()
        {
            return false; // TODO
        }

        public IAudioSourcePlugin GetAudioSourcePlugin(string name)
        {
            if (!audioPlugins.ContainsKey(name)) throw new ArgumentException($"AudioSourcePlugin with name {name} was not found.");
            return audioPlugins[name];
        }

        public IVideoSourcePlugin GetVideoSourcePlugin(string name)
        {
            if (!videoPlugins.ContainsKey(name)) throw new ArgumentException($"VideoSourcePlugin with name {name} was not found.");
            return videoPlugins[name];
        }

        public ISubtitlePlugin GetSubtitlePlugin(string name)
        {
            if (!subtitlePlugins.ContainsKey(name)) throw new ArgumentException($"SubtitlePlugin with name {name} was not found.");
            return subtitlePlugins[name];
        }

        public List<IAudioSourcePlugin> AudioSourcePlugins => audioPlugins.Values.ToList();
        public List<IVideoSourcePlugin> VideoSourcePlugins => videoPlugins.Values.ToList();
        public List<ISubtitlePlugin> SubtitlePlugins => subtitlePlugins.Values.ToList();

        public PluginHandler(string baseDirectory)
        {
            pluginRoot = Path.Combine(baseDirectory, "plugins");
            audioRoot = Path.Combine(pluginRoot, "a");
            videoRoot = Path.Combine(pluginRoot, "v");
            subtitleRoot = Path.Combine(pluginRoot, "s");
            audioPlugins = new Dictionary<string, IAudioSourcePlugin>();
            videoPlugins = new Dictionary<string, IVideoSourcePlugin>();
            subtitlePlugins = new Dictionary<string, ISubtitlePlugin>();
        }
    }
}
