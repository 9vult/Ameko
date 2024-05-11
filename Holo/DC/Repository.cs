using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Holo.DC
{
    public class Repository
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Version { get; set; }
        public List<string>? Repositories { get; set; }
        public List<ScriptEntity>? Scripts { get; set; }
        public string? Url { get; set; }

        public static async Task<Repository?> Build(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var repo = await client.GetFromJsonAsync<Repository>(url);
                    if (repo != null) repo.Url = url;
                    return repo;
                }
                catch (HttpRequestException)
                {
                    HoloContext.Logger.Error($"Error fetching data from {url}", "Repository");
                    return null;
                }
                catch (JsonException)
                {
                    HoloContext.Logger.Error($"Error parsing JSON from {url}", "Repository");
                    return null;
                }
                catch (Exception e)
                {
                    HoloContext.Logger.Error($"Error at {url}: {e.Message}", "Repository");
                    return null;
                }
            }
        }
    }

    public class ScriptEntity
    {
        public string? Name { get; set; }
        public string? QualifiedName { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public double CurrentVersion { get; set; }
        public List<string>? Dependencies { get; set; }
        public string? Url { get; set; }
        public string? Repository { get; set; }
    }
}
