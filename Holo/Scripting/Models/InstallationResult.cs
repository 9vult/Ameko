// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting.Models;

/// <summary>
/// Dependency Control installation results
/// </summary>
public enum InstallationResult
{
    /// <summary>
    /// The package was installed successfully
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
    /// The package is already installed
    /// </summary>
    AlreadyInstalled,

    /// <summary>
    /// The package is not installed
    /// </summary>
    NotInstalled,

    /// <summary>
    /// An error occured with disk I/O
    /// </summary>
    FilesystemFailure,

    /// <summary>
    /// The package is a dependency of another installed package
    /// </summary>
    IsRequiredDependency,

    /// <summary>
    /// The package's Qualified Name is invalid
    /// </summary>
    InvalidName,
}
