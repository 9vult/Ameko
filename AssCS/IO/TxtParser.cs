// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;
using AssCS.History;

namespace AssCS.IO;

/// <summary>
/// Parse a text file to an ass document
/// </summary>
/// <param name="commentDelim">Identifier for comment lines</param>
/// <param name="actorDelim">Delimiter for actors</param>
public class TxtParser(char commentDelim = '#', char actorDelim = ':') : FileParser
{
    protected override Document Parse(TextReader reader)
    {
        var doc = new Document(false);
        doc.StyleManager.LoadDefault();
        doc.ScriptInfoManager.LoadDefault();

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

        if (doc.EventManager.Count == 0)
            doc.EventManager.LoadDefault();

        doc.HistoryManager.Commit(ChangeType.Initial, [doc.EventManager.Head.Id]);

        return doc;
    }
}
