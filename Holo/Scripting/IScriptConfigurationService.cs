// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting;

public interface IScriptConfigurationService
{
    bool TryGet<T>(IHoloExecutable caller, string key, out T? value);
    void Set<T>(IHoloExecutable caller, string key, T value);
    bool Remove(IHoloExecutable caller, string key);
    bool Contains(IHoloExecutable caller, string key);
}
