// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Holo.Configuration;
using Holo.Providers;
using Holo.Scripting;
using NLog;

namespace Ameko.Services;

/// <summary>
/// Provides a controlled service locator for use by
/// <see cref="HoloScript"/> and <see cref="HoloLibrary"/> instances
/// </summary>
/// <remarks>
/// This locator acts as a bridge for user scripts and libraries,
/// allowing them to access a predefined set of application services
/// since they are unable to leverage dependency injection.
/// <para>
/// Only services explicitly added to the locator are available.
/// Any attempt by a script to retrieve an unregistered or unpermitted service will
/// result in an <see cref="System.ArgumentOutOfRangeException"/>.
/// </para>
/// </remarks>
public sealed class ScriptServiceLocator
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Dictionary<Type, object>? Services { get; set; }

    /// <summary>
    /// Retrieves a registered service instance of the specified type.
    /// </summary>
    /// <remarks>
    /// This method is intended for use by dynamically-loaded user scripts and libraries
    /// that cannot utilize traditional dependency injection.
    /// It provides controlled access to a curated set of application services.
    /// </remarks>
    /// <param name="callerName">
    /// Automatically populated with the name of the calling member (method or property)
    /// </param>
    /// <typeparam name="T">
    /// The type of the service to retrieve. Must be a non-nullable reference type.
    /// </typeparam>
    /// <returns>
    /// An instance of the requested service, cast to type <typeparamref name="T"/>
    /// </returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if the requested service type (<typeparamref name="T"/>) is not
    /// registered or is not permitted for access by scripts through this locator
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if this method is called before the locator is initialized
    /// </exception>
    /// <example>
    /// Example usage in a <see cref="HoloScript"/>:
    /// <code>
    /// var slnProvider = ScriptServiceLocator.Get&lt;ISolutionProvider&gt;();
    /// var workingSpace = slnProvider.Current.WorkingSpace;
    /// </code>
    /// </example>
    public static T Get<T>([CallerMemberName] string callerName = "")
        where T : notnull
    {
        if (Services is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ScriptServiceLocator)} is not initialized."
            );
        }

        if (Services.TryGetValue(typeof(T), out var service))
            return (T)service;

        Logger.Warn($"{callerName} attempted to get restricted service {typeof(T)}");
        throw new ArgumentOutOfRangeException(
            nameof(T),
            string.Format(I18N.Other.ScriptServiceLocator_Get_UnregisteredScript, typeof(T).Name)
        );
    }

    internal ScriptServiceLocator(
        IConfiguration configuration,
        IGlobals globals,
        ISolutionProvider solutionProvider,
        IScriptConfigurationService scriptConfigurationService
    )
    {
        Services = new Dictionary<Type, object>
        {
            [typeof(IConfiguration)] = configuration,
            [typeof(IGlobals)] = globals,
            [typeof(ISolutionProvider)] = solutionProvider,
            [typeof(IScriptConfigurationService)] = scriptConfigurationService,
        };
    }
}
