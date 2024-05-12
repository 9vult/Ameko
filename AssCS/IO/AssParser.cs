using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AssCS.IO
{
    /// <summary>
    /// Concrete implementation of a <cref>IFileParser</cref> for .ASS files.
    /// This implementation targets ASS V4+ files using Aegisub headers.
    /// </summary>
    public class AssParser : IFileParser
    {
        public File Load(string filepath)
        {
            ParseFunc parseState = ParseUnknown;
            File assFile = new File();

            using var ioFile = System.IO.File.Open(filepath, System.IO.FileMode.Open);
            var reader = new System.IO.StreamReader(ioFile);
            while (reader.ReadLine() is { } line)
            {
                if (line.Equals(string.Empty)) continue;

                var headerRegex = @"^\[(.+)\]";
                var match = Regex.Match(line, headerRegex);
                if (match.Success)
                {
                    var upper = match.Groups[1].Value.ToUpperInvariant();
                    parseState = upper switch
                    {
                        "V4 STYLES" => ParseStyle, // Yeah idk
                        "V4+ STYLES" => ParseStyle,
                        "V4++ STYLES" => ParseStyle,
                        "EVENTS" => ParseEvent,
                        "SCRIPT INFO" => ParseScriptInfo,
                        "AEGISUB PROJECT GARBAGE" => ParseProjectProperties,
                        "AEGISUB EXTRADATA" => ParseExtradata,
                        "GRAPHICS" => ParseUnknown, // TODO
                        "FONTS" => ParseUnknown, // TODO (Attachments)
                        _ => ParseUnknown
                    };
                    continue; // Skip further processing of this line
                }
                 parseState(line, assFile);
            }

            return assFile;
        }

        private delegate void ParseFunc(string line, File file);

        private void ParseStyle(string line, File file)
        {
            // TODO: Version parsing for v4.00 and v4.00+
            // Will involve adding additional parameters to ToAss() functions
            if (file.Version != FileVersion.V400P) return;

            if (!line.StartsWith("Style:")) return;
            file.StyleManager.Add(
                new Style(file.StyleManager.NextId, line)
            );
        }

        private void ParseEvent(string line, File file)
        {
            if (!line.StartsWith("Dialogue:") && !line.StartsWith("Comment:")) return;
            file.EventManager.AddLast(
                new Event(file.EventManager.NextId, line)
            );
        }

        private void ParseScriptInfo(string line, File file)
        {
            // This block can have comments
            if (line.StartsWith(';')) return;

            var pair = line.Split(":");
            if (pair.Length >= 2)
            {
                file.InfoManager.Set(
                    pair[0].Trim(),
                    string.Join(":", pair, 1, pair.Length - 1).Trim()
                );
            }

            if (pair[0].Trim().ToUpperInvariant().Equals("SCRIPTTYPE"))
                file.Version = pair[1].Trim() switch
                {
                    "v4.00" => FileVersion.V400,
                    "v4.00+" => FileVersion.V400P,
                    "v4.00++" => FileVersion.V400PP,
                    _ => FileVersion.UNKNOWN
                };

            // Not a key:value pair
            else return;
        }

        private void ParseProjectProperties(string line, File file)
        {
            var pair = line.Split(":");
            if (pair.Length >= 2)
            {
                file.PropertiesManager.Set(
                    pair[0].Trim(),
                    string.Join(":", pair, 1, pair.Length - 1).Trim()
                );
            }
            // Not a key:value pair
            else return;
        }

        /// <summary>
        /// AssCS Extradata is written with the format char 'b', and the value is always base64 encoded.
        /// This breaks compatibility with Aegisub Extradata, which uses 'e' and 'u' for Inline and UU encoding.
        /// Aegisub Extradata-encoded lines will be discarded during parsing.
        /// </summary>
        /// <param name="line">Incoming line from the file</param>
        /// <param name="file">File to write parsed data to</param>
        private void ParseExtradata(string line, File file)
        {
            if (!line.StartsWith("Data:")) return;
            var extraRegex = @"^Data:\ *(\d+),([^,]+),(.)(.*)";
            var match = Regex.Match(line, extraRegex);
            if (!match.Success) return;
            int id = Convert.ToInt32(match.Groups[1].Value);
            string key = Utilities.InlineDecode(match.Groups[2].Value);
            string valueType = match.Groups[3].Value;
            string value = valueType switch
            {
                "b" => Utilities.Base64Decode(match.Groups[4].Value),
                // "e" => Utilities.InlineDecode(match.Groups[4].Value),
                // "u" => Convert.ToString(Utilities.UUDecode(match.Groups[4].Value)),
                _ => String.Empty
            };
            // Skip "empty" lines
            if (value.Equals(String.Empty)) return;

            // Ensure the next ID will be larger than the largest existing ID
            file.ExtradataManager.NextId = Math.Max(id + 1, file.ExtradataManager.NextId);
            file.ExtradataManager.Add(new Extradata(id, 0, key, value));
        }

        private void ParseUnknown(string line, File file)
        {
            // Do nothing (for now)
            return;
        }
    }
}
