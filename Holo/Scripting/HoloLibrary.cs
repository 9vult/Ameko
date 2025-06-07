// SPDX-License-Identifier: MPL-2.0

using Holo.Scripting.Models;
using NLog;

namespace Holo.Scripting;

/// <summary>
/// Base class for user script libraries
/// </summary>
/// <remarks>
/// <para>
/// Libraries can be imported by scripts using the <c>css_include</c> directive.
/// </para><para>
/// <b>Note that CS-Script does not re-use library instances!</b>
/// If you need a library to be shared, you need to publish
/// the library on NuGet and use the <c>css_nuget</c> import directive instead.
/// </para>
/// </remarks>
public abstract class HoloLibrary
{
    /// <summary>
    /// Basic script information
    /// </summary>
    public virtual ModuleInfo Info { get; }

    /// <summary>
    /// Initialize the library
    /// </summary>
    /// <param name="info">Library information</param>
    protected HoloLibrary(ModuleInfo info)
    {
        Info = info;
        Logger = LogManager.GetLogger(info.QualifiedName ?? GetType().Name);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected ILogger Logger { get; }
}
