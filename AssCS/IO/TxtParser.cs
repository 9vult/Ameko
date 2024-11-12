// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;

namespace AssCS.IO;

/// <summary>
/// Parse a text file to an ass document
/// </summary>
/// <param name="commentDelim">Identifier for comment lines</param>
/// <param name="actorDelim">Delimiter for actors</param>
public class TxtParser(char commentDelim = '#', char actorDelim = ':') : FileParser
{
    public override Document Parse(TextReader reader)
    {
        var doc = new Document(true);

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            var isComment = false;
            var actor = string.Empty;

            if (line.StartsWith(commentDelim))
            {
                line = line[1..].Trim();
                isComment = true;
            }
            else
            {
                var actorRegex = $@"^(.*){actorDelim} (.+)";
                var match = Regex.Match(line, actorRegex);
                if (match.Success)
                {
                    actor = match.Groups[1].Value;
                    line = match.Groups[2].Value;
                }
            }

            var e = new Event(doc.EventManager.NextId)
            {
                Text = line,
                IsComment = isComment,
                Actor = actor,
                Start = Time.FromSeconds(0),
                End = Time.FromSeconds(0),
            };
            doc.EventManager.AddLast(e);
        }
        return doc;
    }
}
