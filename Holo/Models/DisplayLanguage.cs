// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Language for display use
/// </summary>
/// <param name="Name">Name of the language</param>
/// <param name="Locale">Language's locale code</param>
public readonly record struct DisplayLanguage(string Name, string Locale);
