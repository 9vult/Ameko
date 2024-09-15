namespace LibassCS
{
    public class Libass
    {
        private static bool _initialized;

        /// <summary>
        /// If the library is initialized
        /// </summary>
        public static bool Initialized => _initialized;

        public static void StartUp()
        {
            if (_initialized) return;

            IntPtr library = External.InitLibrary();
            External.UninitLibrary(library);
        }
    }
}
