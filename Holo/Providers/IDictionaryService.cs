// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using Holo.Models;

namespace Holo.Providers;

public interface IDictionaryService
{
    /// <summary>
    /// Try to get a dictionary
    /// </summary>
    /// <param name="lang">Language code</param>
    /// <param name="dictionary">Dictionary, if found</param>
    /// <returns><see langword="true"/> if found</returns>
    bool TryGetDictionary(
        string lang,
        [NotNullWhen(true)] out SpellcheckDictionary? dictionary
    );

    /// <summary>
    /// Download a new dictionary
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    Task<bool> DownloadDicationary(string lang);
}
