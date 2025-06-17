// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting.Models;

/// <summary>
/// Dependency Control installation results
/// </summary>
public enum InstallationResult
{
    /// <summary>
    /// The module was installed successfully
    /// </summary>
    Success,

    /// <summary>
    /// Generic failure
    /// </summary>
    Failure,

    /// <summary>
    /// A dependency could not be found
    /// </summary>
    DependencyNotFound,

    /// <summary>
    /// The module is already installed
    /// </summary>
    AlreadyInstalled,

    /// <summary>
    /// The module is not installed
    /// </summary>
    NotInstalled,

    /// <summary>
    /// An error occured with disk I/O
    /// </summary>
    FilesystemFailure,

    /// <summary>
    /// The module is a dependency of another installed module
    /// </summary>
    IsRequiredDependency,

    /// <summary>
    /// The module's Qualified Name is invalid
    /// </summary>
    InvalidName,
}
