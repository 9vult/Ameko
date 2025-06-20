// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

public struct GitBlame
{
    public required string CommitSha { get; init; }
    public required string Message { get; init; }
    public required string Author { get; init; }
    public required DateTimeOffset Date { get; init; }
}
