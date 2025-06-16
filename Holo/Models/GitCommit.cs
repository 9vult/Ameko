// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

public class GitCommit(
    string author,
    string email,
    string message,
    DateTimeOffset date,
    bool isMerge
)
{
    public string Author { get; private set; } = author;
    public string Email { get; private set; } = email;
    public string Message { get; private set; } = message;
    public DateTimeOffset Date { get; private set; } = date;
    public bool IsMerge { get; private set; } = isMerge;
}
