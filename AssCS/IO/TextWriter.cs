using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssCS.IO
{
    public class TextWriter : IFileWriter
    {
        private readonly File file;
        private readonly string filepath;
        private readonly Encoding encoding;
        private readonly ConsumerInfo consumer;

        public void Write(bool export)
        {
            using var ioFile = System.IO.File.Open(filepath, System.IO.FileMode.OpenOrCreate);
            var writer = new System.IO.StreamWriter(ioFile, encoding);
            writer.WriteLine($"# Exported by {consumer.Name} {consumer.Version} [AssCS]");

            foreach (var line in file.EventManager.Ordered)
            {
                if (line.Comment) continue;
                var actor = line.Actor.Trim().Equals(string.Empty) ? "" : $"{line.Actor.Trim()}: ";
                writer.WriteLine($"{actor}{line.GetStrippedText().Replace("\\N", " ").Replace("\\n", " ")}");
            }

            writer.Flush();
            ioFile.SetLength(ioFile.Position);
            writer.Close();
            ioFile.Close();
        }

        public TextWriter(File file, string filepath, ConsumerInfo consumer) : this(file, filepath, consumer, Encoding.UTF8) { }
        public TextWriter(File file, string filepath, ConsumerInfo consumer, Encoding encoding)
        {
            this.file = file;
            this.filepath = filepath;
            this.encoding = encoding;
            this.consumer = consumer;
        }
    }
}
