using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ameko.DataModels
{
    public class TabLayout
    {
        public string Name { get; internal set; } = string.Empty;
        public string Author { get; internal set; } = string.Empty;
        public string Columns { get; internal set; } = string.Empty;
        public string Rows { get; internal set; } = string.Empty;

        public bool Video { get; internal set; }
        public int VideoColumn { get; internal set; }
        public int VideoRow {  get; internal set; }
        public int VideoColumnSpan { get; internal set; } = 1;
        public int VideoRowSpan { get; internal set; } = 1;

        public bool Audio { get; internal set; }
        public int AudioColumn { get; internal set; }
        public int AudioRow { get; internal set; }
        public int AudioColumnSpan { get; internal set; } = 1;
        public int AudioRowSpan { get; internal set; } = 1;

        public bool Editor { get; internal set; }
        public int EditorColumn { get; internal set; }
        public int EditorRow { get; internal set; }
        public int EditorColumnSpan { get; internal set; } = 1;
        public int EditorRowSpan { get; internal set; } = 1;

        public bool Events { get; internal set; }
        public int EventsColumn { get; internal set; }
        public int EventsRow { get; internal set; }
        public int EventsColumnSpan { get; internal set; } = 1;
        public int EventsRowSpan { get; internal set; } = 1;

        public List<Splitter> Splitters { get; internal set; } = [];

        public static TabLayout Default => new()
        {
            Name = "Default",
            Author = "9volt",
            Columns = "*, 4, *",
            Rows = "0.5*, 4, *, 4, *",

            Video = true,
            VideoColumn = 0,
            VideoRow = 0,
            VideoRowSpan = 3,

            Audio = true,
            AudioColumn = 2,
            AudioRow = 0,

            Editor = true,
            EditorColumn = 2,
            EditorRow = 2,

            Events = true,
            EventsColumn = 0,
            EventsRow = 4,
            EventsColumnSpan = 3,

            Splitters = [
                new() { Vertical = false, Column = 2, Row = 1 },
                new() { Vertical = false, Column = 0, Row = 3, ColumnSpan = 3 },
                new() { Vertical = true,  Column = 1, Row = 0, RowSpan = 3 }
            ]
        };

        public class Splitter
        {
            public bool Vertical { get; internal set; }
            public int Column { get; internal set; }
            public int Row { get; internal set; }
            public int ColumnSpan { get; internal set; } = 1;
            public int RowSpan { get; internal set; } = 1;
        }
    }
}
