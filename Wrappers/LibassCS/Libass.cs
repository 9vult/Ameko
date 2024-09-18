namespace LibassCS
{
    public class Libass
    {
        private static bool _initialized;
        private static unsafe NativeLibrary* _library;
        private static unsafe NativeRenderer* _renderer;

        /// <summary>
        /// If the library is initialized
        /// </summary>
        public static bool Initialized => _initialized;

        /// <summary>
        /// Initialize libass
        /// </summary>
        /// <exception cref="Exception">If initialization fails</exception>
        /// <remarks>This method must be called prior to usage of any other libass function</remarks>
        public static void StartUp()
        {
            if (_initialized) return;

            try
            {
                unsafe
                {
                    _library = External.InitLibrary();
                    _renderer = External.InitRenderer(_library);
                }
                _initialized = true;
            }
            catch (Exception e)
            {
                throw new Exception("An error occured while loading libass!", e);
            }
        }

        /// <summary>
        /// Deinitialize libass
        /// </summary>
        /// <exception cref="Exception">If deinitialization fails</exception>
        public static void Shutdown()
        {
            if (!_initialized) return;

            try
            {
                unsafe
                {
                    External.UninitRenderer(_renderer);
                    External.UninitLibrary(_library);
                }
                _initialized = false;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to unitialize libass!", e);
            }
        }

        /// <summary>
        /// Get the libass version
        /// </summary>
        /// <returns></returns>
        public static int GetVersion()
        {
            if (!Initialized) return -1;
            return External.GetVersion();
        }
    }
}
