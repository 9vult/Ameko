// SPDX-License-Identifier: MPL-2.0

using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Text.Json;
using NLog;

namespace Holo.DependencyControl.Models;

/// <summary>
/// A repository is a collection of <see cref="Module"/>s
/// and <see cref="Repositories"/> for use in Dependency Control
/// </summary>
public record Repository
{
    /// <summary>
    /// Name of the repository
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Short description of the repository
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Name of the person(s) responsible for maintaining the repository
    /// </summary>
    public required string Maintainer { get; init; }

    /// <summary>
    /// <see langword="true"/> if this repository is for beta modules
    /// </summary>
    public required bool IsBetaChannel { get; init; }

    /// <summary>
    /// Modules hosted by this repository
    /// </summary>
    public required FrozenSet<Module> Modules { get; init; }

    /// <summary>
    /// Repositories hoisted by this repository
    /// </summary>
    /// <remarks>Repository json URLs</remarks>
    public required FrozenSet<string> Repositories { get; init; }

    /// <summary>
    /// Automatic property
    /// </summary>
    public string? Url { get; private set; }

    //
    //

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Build a repository
    /// </summary>
    /// <param name="url">Url to download repository data from</param>
    /// <returns><see cref="Repository"/> object, or <see langword="null"/> on failure</returns>
    public static async Task<Repository?> Build(string url)
    {
        using HttpClient client = new HttpClient();
        try
        {
            string content = await client.GetStringAsync(url);
            return Build(url, content);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return null;
        }
    }

    /// <summary>
    /// Build a repository
    /// </summary>
    /// <param name="url">Url of the repository</param>
    /// <param name="jsonContent">JSON content to parse</param>
    /// <returns><see cref="Repository"/> object, or <see langword="null"/> on failure</returns>
    public static Repository? Build(string url, string jsonContent)
    {
        try
        {
            var repo = JsonSerializer.Deserialize<Repository>(jsonContent);
            if (repo is not null)
                repo.Url = url;
            return repo;
        }
        catch (JsonException e)
        {
            Logger.Error($"Failed to parse json from {url}");
            Logger.Error(e);
            return null;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return null;
        }
    }
}
