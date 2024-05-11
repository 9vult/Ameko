using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Holo.DC
{
    /// <summary>
    /// Manages scripts on the filesystem for Dependency Control
    /// </summary>
    public class DCScriptManager
    {
        private static readonly string scriptRoot = Path.Combine(HoloContext.Directories.HoloDataHome, "scripts");

        /// <summary>
        /// Install a script
        /// </summary>
        /// <param name="entity">Script entity</param>
        /// <returns>True if installation was successful</returns>
        public static async Task<bool> InstallDCScript(ScriptEntity entity)
        {
            var qualifiedName = entity.QualifiedName ?? string.Empty;
            var updateUrl = entity.Url ?? string.Empty;
            var repo = entity.Repository ?? string.Empty;

            if (IsDCScriptInstalled(qualifiedName!)) return false;
            using (var client = new HttpClient())
            {
                try
                {
                    using (var stream = await client.GetStreamAsync(updateUrl))
                    { 
                        using (var fs = new FileStream(ScriptPath(qualifiedName), FileMode.OpenOrCreate))
                        {
                            await stream.CopyToAsync(fs);
                        }
                    }
                    HoloContext.Instance.ConfigurationManager.AddScript(repo, qualifiedName);
                    return true;
                }
                catch
                {
                    HoloContext.Logger.Error($"An error occured while installing {qualifiedName}.", "DCScriptManager");
                    return false;
                }
            }
        }

        /// <summary>
        /// Uninstall a script
        /// </summary>
        /// <param name="entity">Script entity</param>
        /// <returns>True if the script was removed successfully</returns>
        public static bool UninstallDCScript(ScriptEntity entity)
        {
            var qualifiedName = entity.QualifiedName ?? string.Empty;
            if (IsDCScriptInstalled(qualifiedName))
                try
                {
                    File.Delete(ScriptPath(qualifiedName));
                    HoloContext.Instance.ConfigurationManager.RemoveScript(qualifiedName);
                    return true;
                }
                catch
                {
                    HoloContext.Logger.Error($"An error occured while deleting {qualifiedName}.", "DCScriptManager");
                    return false;
                }
            return false;
        }

        /// <summary>
        /// Update a script
        /// </summary>
        /// <param name="entity">Script entity</param>
        /// <returns></returns>
        public static async Task<bool> UpdateDCScript(ScriptEntity entity)
        {
            if (UninstallDCScript(entity))
                return await InstallDCScript(entity);
            return false;
        }

        /// <summary>
        /// Check if a script is installed
        /// </summary>
        /// <param name="qualifiedName">Name of the script</param>
        /// <returns>True if the script is installed</returns>
        public static bool IsDCScriptInstalled(string qualifiedName)
        {
            return File.Exists(ScriptPath(qualifiedName));
        }

        /// <summary>
        /// Get the candidate scripts for updating
        /// </summary>
        /// <param name="serverScripts">List of scripts on the server</param>
        /// <param name="localScripts">List of installed scripts</param>
        /// <returns>List of server scripts that are update candidates</returns>
        public static List<ScriptEntity> GetUpdateCandidates(List<ScriptEntity> serverScripts, List<ScriptEntity> localScripts)
        {
            var updates = new List<ScriptEntity>();

            foreach (var script in localScripts)
            {
                var matches = serverScripts.Where(s => s.QualifiedName!.Equals(script.QualifiedName));
                if (matches.Any())
                {
                    var serverScript = matches.First();
                    if (serverScript.CurrentVersion > script.CurrentVersion)
                        updates.Add(serverScript);
                }
            }
            return updates;
        }

        /// <summary>
        /// Get the filepath corresponding to the script
        /// </summary>
        /// <param name="qualifiedName">Name of the script</param>
        /// <returns>The filepath, ending in .cs</returns>
        private static string ScriptPath(string qualifiedName)
        {
            return Path.Combine(scriptRoot, $"{qualifiedName}.cs");
        }

        private DCScriptManager()
        {
        }
    }
}
