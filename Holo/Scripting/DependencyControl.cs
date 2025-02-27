// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Text.Json;
using Holo.IO;
using Holo.Scripting.Models;
using NLog;

namespace Holo.Scripting;

/// <summary>
/// Dependency Control manages <see cref="Repository"/>s and <see cref="Module"/>s
/// </summary>
public class DependencyControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };
    private static readonly Uri ModulesRoot = new Uri(
        Path.Combine(Directories.DataHome, "scripts")
    );

    private IFileSystem _fileSystem;
    private Repository? _baseRepository;
    private readonly Dictionary<string, Repository> _repositoryMap;
    private readonly Dictionary<string, Module> _moduleMap;
    private readonly ObservableCollection<Repository> _repositories;
    private readonly ObservableCollection<Module> _moduleStore;

    private readonly ObservableCollection<Module> _installedModules;

    /// <summary>
    /// List of available repositories
    /// </summary>
    public ReadOnlyObservableCollection<Repository> Repositories { get; }

    /// <summary>
    /// List of available modules
    /// </summary>
    public ReadOnlyObservableCollection<Module> ModuleStore { get; }

    /// <summary>
    /// List of installed modules
    /// </summary>
    public ReadOnlyObservableCollection<Module> InstalledModules { get; }

    #region Modules

    public bool IsModuleInstalled(string qualifiedName)
    {
        return _fileSystem.File.Exists(ModulePath(qualifiedName));
    }

    /// <summary>
    /// Recursively install a <see cref="Module"/> and its dependencies
    /// </summary>
    /// <param name="module">Module to install</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    public async Task<InstallationResult> InstallModule(Module module)
    {
        if (IsModuleInstalled(module.QualifiedName))
            return InstallationResult.AlreadyInstalled;
        Logger.Info($"Attempting to install module {module.QualifiedName}");

        foreach (
            var dependencyName in module
                .Dependencies.Where(dependencyName => !IsModuleInstalled(dependencyName)) // Skip already-installed modules
                .Where(dependencyName => dependencyName != module.QualifiedName)
        ) // Prevent recursion
        {
            if (!_moduleMap.TryGetValue(dependencyName, out Module? dependency))
            {
                Logger.Error(
                    $"Failed to find dependency {dependencyName} for module {module.QualifiedName}"
                );
                return InstallationResult.DependencyNotFound;
            }

            var depResult = await InstallModule(dependency);
            if (depResult != InstallationResult.Success)
                return depResult; // Cascade failures
        }

        using var client = new HttpClient();
        try
        {
            await using var stream = await client.GetStreamAsync(module.Url);
            await using var fs = _fileSystem.FileStream.New(
                ModulePath(module.QualifiedName),
                FileMode.OpenOrCreate
            );
            await stream.CopyToAsync(fs);
            _installedModules.Add(module);
            Logger.Info($"Successfully installed module {module.QualifiedName}");
            return InstallationResult.Success;
        }
        catch (IOException e)
        {
            Logger.Error($"Failed to write module {module.QualifiedName} to disk");
            Logger.Error(e);
            return InstallationResult.FilesystemFailure;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to install module {module.QualifiedName}");
            Logger.Error(e);
            return InstallationResult.Failure;
        }
    }

    /// <summary>
    /// Uninstall a <see cref="Module"/>
    /// </summary>
    /// <param name="module">Module to uninstall</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    /// <remarks>Does not uninstall dependencies</remarks>
    public InstallationResult UninstallModule(Module module)
    {
        if (!IsModuleInstalled(module.QualifiedName))
            return InstallationResult.NotInstalled;
        if (_installedModules.Any(m => m.Dependencies.Contains(module.QualifiedName)))
            return InstallationResult.IsRequiredDependency;
        Logger.Info($"Attempting to uninstall module {module.QualifiedName}");

        try
        {
            _fileSystem.File.Delete(ModulePath(module.QualifiedName));
            Logger.Info($"Successfully uninstalled module {module.QualifiedName}");
            return InstallationResult.Success;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to uninstall module {module.QualifiedName}");
            Logger.Error(e);
            return InstallationResult.Failure;
        }
    }

    /// <summary>
    /// Update a module to the latest version
    /// </summary>
    /// <param name="module">Module to update</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    public async Task<InstallationResult> UpdateModule(Module module)
    {
        var uninstallResult = UninstallModule(module);
        if (uninstallResult == InstallationResult.Success)
            return await InstallModule(module);
        return uninstallResult;
    }

    public List<Module> GetUpdateCandidates()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the filepath for a module
    /// </summary>
    /// <param name="qualifiedName">Qualified name of the module</param>
    /// <returns>The filepath, ending in <c>.cs</c></returns>
    public static string ModulePath(string qualifiedName)
    {
        return Path.Combine(ModulesRoot.LocalPath, $"{qualifiedName}.cs");
    }

    #endregion Modules

    #region Repositories

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
        _moduleMap.Clear();
        _moduleStore.Clear();
        foreach (var repo in _repositories)
        {
            foreach (var module in repo.Modules)
            {
                module.Repository = repo.Name;
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
    /// Load repositories from a list of URLs and populate the <see cref="ModuleStore"/>
    /// </summary>
    /// <param name="repoUrls">List of <see cref="Repository"/> URLs</param>
    /// <remarks>Does not clear the collections</remarks>
    public async Task BootstrapFromList(List<string> repoUrls)
    {
        foreach (var url in repoUrls)
        {
            var repo = await Repository.Build(url);
            if (repo is not null)
                await GatherRepositories(repo);
        }
        GatherModules();
    }

    /// <summary>
    /// Set up the base repository
    /// </summary>
    private async Task SetUpBaseRepository()
    {
        _baseRepository = await Repository.Build("");
    }

    #endregion Repositories

    /// <summary>
    /// Instantiate a Dependency Control instance
    /// </summary>
    public DependencyControl()
        : this(new FileSystem()) { }

    /// <summary>
    /// Instantiate a Dependency Control instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    public DependencyControl(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _repositoryMap = [];
        _moduleMap = [];
        _repositories = [];
        _moduleStore = [];

        _installedModules = [];

        Repositories = new ReadOnlyObservableCollection<Repository>(_repositories);
        ModuleStore = new ReadOnlyObservableCollection<Module>(_moduleStore);

        InstalledModules = new ReadOnlyObservableCollection<Module>(_installedModules);

        _ = SetUpBaseRepository();
    }
}
