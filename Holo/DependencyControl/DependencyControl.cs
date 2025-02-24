// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Text.Json;
using Holo.DependencyControl.Models;
using Holo.IO;
using NLog;

namespace Holo.DependencyControl;

/// <summary>
/// Dependency Control manages <see cref="Repository"/>s and <see cref="Module"/>s
/// </summary>
public class DependencyControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };
    private static readonly Uri ScriptRoot = new Uri(Path.Combine(Directories.DataHome, "scripts"));

    private Repository? _baseRepository;
    private readonly Dictionary<string, Repository> _repositoryMap;
    private readonly Dictionary<string, Module> _moduleMap;
    private readonly ObservableCollection<Repository> _repositories;
    private readonly ObservableCollection<Module> _moduleStore;

    public ReadOnlyObservableCollection<Repository> Repositories { get; }
    public ReadOnlyObservableCollection<Module> ModuleStore { get; }

    /// <summary>
    /// Recursively collect repositories
    /// </summary>
    /// <param name="repository">Head repository</param>
    public async Task GatherRepositories(Repository repository)
    {
        if (_repositoryMap.TryAdd(repository.Name, repository))
            _repositories.Add(repository);

        foreach (string url in repository.Repositories)
        {
            var repo = await Repository.Build(url);
            if (repo is null)
            {
                Logger.Warn($"Unable to build repository {url}");
                continue;
            }

            if (!_repositoryMap.ContainsKey(repo.Name))
                await GatherRepositories(repo);
        }
    }

    /// <summary>
    /// Populate the <see cref="ModuleStore"/>
    /// </summary>
    public void GatherModules()
    {
        _moduleStore.Clear();
        foreach (var repo in _repositories)
        {
            foreach (var module in repo.Modules)
            {
                module.Repository = repo.Url;
                if (_moduleMap.TryGetValue(module.QualifiedName, out Module? conflict))
                {
                    Logger.Warn(
                        $"Conflict between {module.QualifiedName} from {conflict.Repository} and {module.Repository}"
                    );
                    continue;
                }
                _moduleMap.Add(module.QualifiedName, module);
                _moduleStore.Add(module);
            }
        }
    }

    /// <summary>
    /// Set up the base repository
    /// </summary>
    private async Task SetUpBaseRepository()
    {
        _baseRepository = await Repository.Build("");
    }

    public DependencyControl()
    {
        _repositoryMap = [];
        _moduleMap = [];
        _repositories = [];
        _moduleStore = [];

        Repositories = new ReadOnlyObservableCollection<Repository>(_repositories);
        ModuleStore = new ReadOnlyObservableCollection<Module>(_moduleStore);

        _ = SetUpBaseRepository();
    }
}
