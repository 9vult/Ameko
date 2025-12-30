// SPDX-License-Identifier: MPL-2.0

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
            var data = line.AsSpan().TrimStart();
            if (data.IsEmpty)
                continue;

            var isComment = false;
            var actor = string.Empty;

            if (data[0] == commentDelim)
            {
                data = data[1..].Trim();
                isComment = true;
            }
            else
            {
                var separator = data.IndexOf(actorDelim);
                if (separator >= 0)
                {
                    actor = data[..separator].TrimEnd().ToString();
                    data = data[(separator + 1)..].TrimStart();
                }
            }

            var e = new Event(doc.EventManager.NextId)
            {
                Text = data.ToString(),
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
