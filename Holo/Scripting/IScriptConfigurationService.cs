// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;

namespace Holo.Scripting;

public interface IScriptConfigurationService
{
    bool TryGet<T>(IHoloExecutable caller, string key, [NotNullWhen(true)] out T? value);
    void Set<T>(IHoloExecutable caller, string key, T value);
    bool Remove(IHoloExecutable caller, string key);
    bool Contains(IHoloExecutable caller, string key);

    bool TryGet<T>(
        IHoloExecutable caller,
        Project project,
        string key,
        [NotNullWhen(true)] out T? value
    );
    void Set<T>(IHoloExecutable caller, Project project, string key, T value);
    bool Remove(IHoloExecutable caller, Project project, string key);
    bool Contains(IHoloExecutable caller, Project project, string key);
}
