using AssCS.History;

namespace AssCS
{
    /// <summary>
    /// A single Advanced Substation Alpha subtitle file
    /// </summary>
    public class File
    {
        public FileVersion Version { get; set; }
        public InfoManager InfoManager { get; }
        public StyleManager StyleManager { get; }
        public EventManager EventManager { get; }
        public AttachmentManager AttachmentManager { get; }
        public ExtradataManager ExtradataManager { get; }
        public PropertiesManager PropertiesManager { get; }
        public HistoryManager HistoryManager { get; }

        /// <summary>
        /// Bootstrap this file with default values for a fresh start
        /// </summary>
        public void LoadDefault()
        {
            Version = FileVersion.V400P;
            InfoManager.LoadDefault();
            StyleManager.LoadDefault();
            EventManager.LoadDefault();
            PropertiesManager.LoadDefault();
        }

        /// <summary>
        /// Clone another File instance
        /// </summary>
        /// <param name="source">File to be copied</param>
        public File(File source)
        {
            Version = source.Version;
            InfoManager = new InfoManager(source);
            StyleManager = new StyleManager(source);
            EventManager = new EventManager(source);
            AttachmentManager = new AttachmentManager(source);
            ExtradataManager = new ExtradataManager(source);
            PropertiesManager = new PropertiesManager(source);
            HistoryManager = new HistoryManager();
        }

        /// <summary>
        /// Instantiate a new File instance
        /// </summary>
        public File()
        {
            Version = FileVersion.V400P;
            InfoManager = new InfoManager();
            StyleManager = new StyleManager();
            EventManager = new EventManager();
            AttachmentManager = new AttachmentManager();
            ExtradataManager = new ExtradataManager();
            PropertiesManager = new PropertiesManager();
            HistoryManager = new HistoryManager();
        }
    }

    /// <summary>
    /// ASS File Version
    /// </summary>
    public enum FileVersion
    {
        /// <summary>
        /// v4.00
        /// </summary>
        V400 = 0,
        /// <summary>
        /// v4.00+
        /// </summary>
        V400P = 1,
        /// <summary>
        /// v4.00++
        /// </summary>
        V400PP = 2,
        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN = -1
    }
}
