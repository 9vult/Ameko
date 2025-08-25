// SPDX-License-Identifier: GPL-3.0-only

using System.ComponentModel;
using Holo.Configuration;
using Holo.Providers;

namespace Ameko.Services;

/// <summary>
/// Push applicable options to AssCS on update
/// </summary>
public class AssOptionsBinder
{
    private readonly IConfiguration _configuration;
    private readonly ISolutionProvider _solutionProvider;

    public AssOptionsBinder(IConfiguration configuration, ISolutionProvider solutionProvider)
    {
        _configuration = configuration;
        _solutionProvider = solutionProvider;

        _configuration.PropertyChanged += PushUpdate;
        _solutionProvider.PropertyChanged += (_, _) =>
        {
            _solutionProvider.Current.PropertyChanged += PushUpdate;
        };
    }

    /// <summary>
    /// Blindly update AssCS' Options
    /// </summary>
    /// <remarks>
    /// This does include triggers from changes like <see cref="Holo.Solution.WorkingSpace"/>
    /// but it's unlikely there will ever be enough options / fast enough changes
    /// for that to make any measurable impact on anything.
    /// </remarks>
    private void PushUpdate(object? sender, PropertyChangedEventArgs e)
    {
        var sln = _solutionProvider.Current;

        AssCS.Options.CpsIncludesPunctuation =
            sln.CpsIncludesPunctuation ?? _configuration.CpsIncludesPunctuation;
        AssCS.Options.CpsIncludesWhitespace =
            sln.CpsIncludesWhitespace ?? _configuration.CpsIncludesWhitespace;
        AssCS.Options.DefaultLayer = sln.DefaultLayer ?? _configuration.DefaultLayer;

        // Non-solution
        AssCS.Options.LineWidthIncludesPunctuation = _configuration.LineWidthIncludesPunctuation;
        AssCS.Options.LineWidthIncludesWhitespace = _configuration.LineWidthIncludesWhitespace;
    }
}
