// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Holo.DependencyControl.Models;
using Holo.IO;
using NLog;

namespace Holo.DependencyControl;

public class DependencyControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };
    private static readonly Uri ScriptRoot = new Uri(Path.Combine(Directories.DataHome, "scripts"));

    private Repository? _baseRepository;
    private readonly Dictionary<string, Repository> _repositories;

    private async Task SetUpBaseRepository()
    {
        _baseRepository = await Repository.Build("");
    }

    public DependencyControl()
    {
        _repositories = new Dictionary<string, Repository>();

        _ = SetUpBaseRepository();
    }
}
