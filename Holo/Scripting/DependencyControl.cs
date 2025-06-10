// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Holo.IO;
using Holo.Scripting.Models;
using NLog;

namespace Holo.Scripting;

/// <summary>
/// Dependency Control manages <see cref="Repository"/>s and <see cref="Module"/>s
/// </summary>
public class DependencyControl : IDependencyControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static readonly Uri ModulesRoot = new Uri(
        Path.Combine(Directories.DataHome, "scripts")
    );

    /// <summary>
    /// The filesystem being used
    /// </summary>
    /// <remarks>This allows for filesystem mocking to be used in tests</remarks>
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// The HttpClient being used
    /// </summary>
    /// <remarks>This allows for client mocking to be used in tests</remarks>
    private readonly HttpClient _httpClient;

    private Repository? _baseRepository;
    private readonly Dictionary<string, Repository> _repositoryMap;
    private readonly Dictionary<string, Module> _moduleMap;
    private readonly ObservableCollection<Repository> _repositories;
    private readonly ObservableCollection<Module> _moduleStore;

    private readonly ObservableCollection<Module> _installedModules;

    /// <summary>
    /// Base repository URL
    /// </summary>
    public string BaseRepositoryUrl { get; } = "https://dc.ameko.moe/base.json";

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

    /// <summary>
    /// Determine if a module is installed
    /// </summary>
    /// <param name="module">Module to check</param>
    /// <returns><see langword="true"/> if the module is installed</returns>
    public bool IsModuleInstalled(Module module)
    {
        return _fileSystem.File.Exists(ModulePath(module));
    }

    /// <summary>
    /// Recursively install a <see cref="Module"/> and its dependencies
    /// </summary>
    /// <param name="module">Module to install</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    public async Task<InstallationResult> InstallModule(Module module)
    {
        if (IsModuleInstalled(module))
            return InstallationResult.AlreadyInstalled;
        Logger.Info($"Attempting to install module {module.QualifiedName}");

        foreach (
            var dependencyName in module.Dependencies.Where(dependencyName =>
                dependencyName != module.QualifiedName
            ) // Prevent recursion
        )
        {
            if (!_moduleMap.TryGetValue(dependencyName, out var dependency))
            {
                Logger.Error(
                    $"Failed to find dependency {dependencyName} for module {module.QualifiedName}"
                );
                return InstallationResult.DependencyNotFound;
            }

            // Skip already-installed dependencies
            if (IsModuleInstalled(dependency))
                continue;

            var depResult = await InstallModule(dependency);
            if (depResult != InstallationResult.Success)
                return depResult; // Cascade failures
        }

        try
        {
            var path = ModulePath(module);
            var sidecar = SidecarPath(module);
            // Create module directories if they don't exist
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(sidecar)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(sidecar) ?? "/");

            await using var dlStream = await _httpClient.GetStreamAsync(module.Url);
            await using var moduleFs = _fileSystem.FileStream.New(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            await dlStream.CopyToAsync(moduleFs);

            await using var sidecarFs = _fileSystem.FileStream.New(
                sidecar,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            await JsonSerializer.SerializeAsync(sidecarFs, module, JsonOptions);

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
    /// <param name="isUpdate">Bypass dependency checking for update purposes</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    /// <remarks>Does not uninstall dependencies</remarks>
    public InstallationResult UninstallModule(Module module, bool isUpdate = false)
    {
        if (!IsModuleInstalled(module))
            return InstallationResult.NotInstalled;
        if (!isUpdate && _installedModules.Any(m => m.Dependencies.Contains(module.QualifiedName)))
            return InstallationResult.IsRequiredDependency;
        Logger.Info($"Attempting to uninstall module {module.QualifiedName}");

        try
        {
            _fileSystem.File.Delete(ModulePath(module));
            _fileSystem.File.Delete(SidecarPath(module));
            Logger.Info($"Successfully uninstalled module {module.QualifiedName}");
            _installedModules.Remove(module);
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
        Logger.Info($"Update for module {module.QualifiedName} requested...");
        var uninstallResult = UninstallModule(module, true);
        if (uninstallResult == InstallationResult.Success)
            return await InstallModule(module);
        return uninstallResult;
    }

    /// <summary>
    /// Get a list of installed modules with available updates
    /// </summary>
    /// <returns>List of updatable modules</returns>
    public List<Module> GetUpdateCandidates()
    {
        var updatable = new List<Module>();

        foreach (var module in _installedModules)
        {
            var match = _moduleStore.FirstOrDefault(m => m.QualifiedName == module.QualifiedName);
            if (match is null)
                continue;

            if (match.Version > module.Version)
                updatable.Add(match);
        }
        return updatable;
    }

    /// <summary>
    /// Check if a module is up to date
    /// </summary>
    /// <param name="module">Module to check</param>
    /// <returns><see langword="true"/> if the module is up to date</returns>
    /// <remarks>If the module isn't found in the <see cref="ModuleStore"/>, returns <see langword="true"/></remarks>
    public bool IsModuleUpToDate(Module module)
    {
        var match = _moduleStore.FirstOrDefault(m => m.QualifiedName == module.QualifiedName);
        if (match is null)
            return true;

        return match.Version <= module.Version;
    }

    /// <summary>
    /// Get the filepath for a module
    /// </summary>
    /// <param name="module">Module</param>
    /// <returns>The filepath, ending in <c>.cs</c></returns>
    public static string ModulePath(Module module)
    {
        var qName = module.QualifiedName;
        return module.Type switch
        {
            ModuleType.Script => Path.Combine(ModulesRoot.LocalPath, $"{qName}.cs"),
            ModuleType.Library => Path.Combine(ModulesRoot.LocalPath, $"{qName}.lib.cs"),
            _ => throw new ArgumentOutOfRangeException(nameof(module)),
        };
    }

    /// <summary>
    /// Get the filepath for a module sidecar
    /// </summary>
    /// <param name="module">Module</param>
    /// <returns>The filepath, ending in <c>.json</c></returns>
    public static string SidecarPath(Module module)
    {
        var qName = module.QualifiedName;
        return module.Type switch
        {
            ModuleType.Script => Path.Combine(ModulesRoot.LocalPath, "modules", $"{qName}.json"),
            ModuleType.Library => Path.Combine(
                ModulesRoot.LocalPath,
                "modules",
                $"{qName}.lib.json"
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(module)),
        };
    }

    #endregion Modules

    #region Repositories

    /// <summary>
    /// Recursively collect repositories
    /// </summary>
    /// <param name="repository">Head repository</param>
    /// <returns>List of new repositories</returns>
    private async Task<IList<Repository>> GatherRepositories(Repository repository)
    {
        Logger.Trace($"Gathering repositories for repository '{repository.Name}'");
        var newRepos = new List<Repository>();

        if (_repositoryMap.TryAdd(repository.Name, repository))
        {
            _repositories.Add(repository);
            newRepos.Add(repository);
        }

        foreach (var url in repository.Repositories)
        {
            var repo = await Repository.Build(url, _httpClient);
            if (repo is null)
            {
                Logger.Warn($"Unable to build repository {url}");
                continue;
            }

            if (!_repositoryMap.ContainsKey(repo.Name))
                newRepos.AddRange(await GatherRepositories(repo));
        }
        return newRepos;
    }

    /// <summary>
    /// Populate the <see cref="ModuleStore"/> and the <see cref="InstalledModules"/> list
    /// </summary>
    /// <param name="repositories">Repositories to gather for. Defaults to <see cref="Repositories"/></param>
    private void GatherModules(IList<Repository>? repositories = null)
    {
        Logger.Trace("Gathering modules...");
        foreach (var repo in repositories ?? _repositories)
        {
            Logger.Trace($"Gathering modules in repository '{repo.Name}'");
            foreach (var module in repo.Modules)
            {
                module.Repository = repo.Name;
                if (_moduleMap.TryGetValue(module.QualifiedName, out var conflict))
                {
                    Logger.Warn(
                        $"Conflict between {module.QualifiedName} from '{conflict.Repository}' and '{module.Repository}'"
                    );
                    continue;
                }
                _moduleMap.Add(module.QualifiedName, module);
                _moduleStore.Add(module);

                if (!IsModuleInstalled(module))
                    continue;

                var sidecarPath = SidecarPath(module);
                if (_fileSystem.File.Exists(sidecarPath))
                {
                    try
                    {
                        using var sidecarFs = _fileSystem.FileStream.New(
                            sidecarPath,
                            FileMode.Open
                        );
                        var sidecar = JsonSerializer.Deserialize<Module>(sidecarFs, JsonOptions);
                        if (sidecar is null)
                            Logger.Warn($"Failed to read sidecar {sidecarPath}");
                        _installedModules.Add(sidecar ?? module);
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"Failed to read sidecar {sidecarPath} because of {e}");
                        _installedModules.Add(module);
                    }
                }
                else
                {
                    _installedModules.Add(module);
                }
            }
        }
        Logger.Trace("Finished gathering modules");
    }

    /// <summary>
    /// Load repositories from a list of URLs and populate the <see cref="ModuleStore"/>
    /// </summary>
    /// <param name="repoUrls">List of <see cref="Repository"/> URLs</param>
    public async Task AddAdditionalRepositories(IList<string> repoUrls)
    {
        Logger.Info($"Adding additional {repoUrls.Count} user repositories...");
        List<Repository> newRepos = [];

        foreach (var url in repoUrls)
        {
            var repo = await Repository.Build(url, _httpClient);
            if (repo is not null)
                newRepos.AddRange(await GatherRepositories(repo));
        }
        if (repoUrls.Count > 1)
            GatherModules(newRepos);
    }

    /// <summary>
    /// Set up the base repository
    /// </summary>
    /// <remarks>This clears Dependency Control</remarks>
    public async Task SetUpBaseRepository()
    {
        Logger.Info("Setting up base repository");
        _repositories.Clear();
        _repositoryMap.Clear();
        _installedModules.Clear();
        _moduleStore.Clear();
        _moduleMap.Clear();

        _baseRepository = await Repository.Build(BaseRepositoryUrl, _httpClient);
        if (_baseRepository is not null)
        {
            await GatherRepositories(_baseRepository);
            GatherModules();
        }
    }

    #endregion Repositories

    /// <summary>
    /// Instantiate a Dependency Control instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="httpClient">HttpClient to use</param>
    /// <remarks>
    /// This constructor does not set up the base repository.
    /// <see cref="SetUpBaseRepository"/> should be called following construction
    /// </remarks>
    public DependencyControl(IFileSystem fileSystem, HttpClient httpClient)
    {
        _fileSystem = fileSystem;
        _httpClient = httpClient;
        _repositoryMap = [];
        _moduleMap = [];
        _repositories = [];
        _moduleStore = [];

        _installedModules = [];

        Repositories = new ReadOnlyObservableCollection<Repository>(_repositories);
        ModuleStore = new ReadOnlyObservableCollection<Module>(_moduleStore);

        InstalledModules = new ReadOnlyObservableCollection<Module>(_installedModules);

        Logger.Info("Initialized Dependency Control");
    }
}
