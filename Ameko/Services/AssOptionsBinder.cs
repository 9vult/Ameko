// SPDX-License-Identifier: GPL-3.0-only

using System.ComponentModel;
using Holo;
using Holo.Configuration;
using Holo.Providers;

namespace Ameko.Services;

/// <summary>
/// Push applicable options to AssCS on update
/// </summary>
public class AssOptionsBinder
{
    private readonly IConfiguration _configuration;
    private readonly IProjectProvider _projectProvider;

    public AssOptionsBinder(IConfiguration configuration, IProjectProvider projectProvider)
    {
        _configuration = configuration;
        _projectProvider = projectProvider;

        _configuration.PropertyChanged += PushUpdate;
        _projectProvider.PropertyChanged += (_, _) =>
        {
            _projectProvider.Current.PropertyChanged += PushUpdate;
        };
    }

    /// <summary>
    /// Blindly update AssCS' Options
    /// </summary>
    /// <remarks>
    /// This does include triggers from changes like <see cref="Project.WorkingSpace"/>
    /// but it's unlikely there will ever be enough options / fast enough changes
    /// for that to make any measurable impact on anything.
    /// </remarks>
    private void PushUpdate(object? sender, PropertyChangedEventArgs e)
    {
        var prj = _projectProvider.Current;

        AssCS.Options.CpsIncludesPunctuation =
            prj.CpsIncludesPunctuation ?? _configuration.CpsIncludesPunctuation;
        AssCS.Options.CpsIncludesWhitespace =
            prj.CpsIncludesWhitespace ?? _configuration.CpsIncludesWhitespace;
        AssCS.Options.DefaultLayer = prj.DefaultLayer ?? _configuration.DefaultLayer;

        // Non-project
        AssCS.Options.LineWidthIncludesPunctuation = _configuration.LineWidthIncludesPunctuation;
        AssCS.Options.LineWidthIncludesWhitespace = _configuration.LineWidthIncludesWhitespace;
    }
}
