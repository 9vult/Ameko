// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using Holo.Scripting.Models;

namespace Holo.Scripting;

public interface IPackageManager
{
    /// <summary>
    /// Base repository URL
    /// </summary>
    string BaseRepositoryUrl { get; }

    /// <summary>
    /// List of available repositories
    /// </summary>
    ReadOnlyObservableCollection<Repository> Repositories { get; }

    /// <summary>
    /// List of available modules
    /// </summary>
    ReadOnlyObservableCollection<Module> ModuleStore { get; }

    /// <summary>
    /// List of installed modules
    /// </summary>
    ReadOnlyObservableCollection<Module> InstalledModules { get; }

    /// <summary>
    /// Determine if a module is installed
    /// </summary>
    /// <param name="module">Module to check</param>
    /// <returns><see langword="true"/> if the module is installed</returns>
    bool IsModuleInstalled(Module module);

    /// <summary>
    /// Recursively install a <see cref="Module"/> and its dependencies
    /// </summary>
    /// <param name="module">Module to install</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    Task<InstallationResult> InstallModule(Module module);

    /// <summary>
    /// Uninstall a <see cref="Module"/>
    /// </summary>
    /// <param name="module">Module to uninstall</param>
    /// <param name="isUpdate">Bypass dependency checking for update purposes</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    /// <remarks>Does not uninstall dependencies</remarks>
    InstallationResult UninstallModule(Module module, bool isUpdate = false);

    /// <summary>
    /// Update a module to the latest version
    /// </summary>
    /// <param name="module">Module to update</param>
    /// <returns><see cref="InstallationResult.Success"/> on success</returns>
    Task<InstallationResult> UpdateModule(Module module);

    /// <summary>
    /// Get a list of installed modules with available updates
    /// </summary>
    /// <returns>List of updatable modules</returns>
    List<Module> GetUpdateCandidates();

    /// <summary>
    /// Check if a module is up to date
    /// </summary>
    /// <param name="module">Module to check</param>
    /// <returns><see langword="true"/> if the module is up to date</returns>
    /// <remarks>If the module isn't found in the <see cref="ModuleStore"/>, returns <see langword="true"/></remarks>
    bool IsModuleUpToDate(Module module);

    /// <summary>
    /// Load repositories from a list of URLs and populate the <see cref="ModuleStore"/>
    /// </summary>
    /// <param name="repoUrls">List of <see cref="Repository"/> URLs</param>
    Task AddAdditionalRepositories(IList<string> repoUrls);

    /// <summary>
    /// Add a repository and populate the <see cref="ModuleStore"/>
    /// </summary>
    /// <param name="repoUrl"><see cref="Repository"/> URL</param>
    /// <returns><see langword="true"/> if the addition was successful</returns>
    Task<InstallationResult> AddRepository(string repoUrl);

    /// <summary>
    /// Remove a repository and update the <see cref="ModuleStore"/>
    /// </summary>
    /// <param name="repoName">Name of the repository to remove</param>
    /// <returns><see langword="true"/> if the removal was successful</returns>
    InstallationResult RemoveRepository(string repoName);

    /// <summary>
    /// Set up the base repository
    /// </summary>
    /// <remarks>This clears Dependency Control</remarks>
    Task SetUpBaseRepository();
}
