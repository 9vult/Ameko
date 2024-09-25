using LibassCS.Structures;

namespace LibassCS
{
    public class Libass
    {
        private static bool _initialized;
        private static unsafe NativeLibrary* _library;

        private static readonly List<Renderer> _renderers = [];

        /// <summary>
        /// If the library is initialized
        /// </summary>
        public static bool Initialized => _initialized;

        /// <summary>
        /// Create a new renderer
        /// </summary>
        /// <returns>Renderer wrapper</returns>
        /// <exception cref="InvalidOperationException">If libass was not initialized beforehand</exception>
        public static unsafe Renderer CreateRenderer()
        {
            if (!Initialized) throw new InvalidOperationException("Libass was not initialized prior to renderer creation");
            NativeRenderer* nrenderer = External.InitRenderer(_library);
            Renderer renderer = new(nrenderer);
            _renderers.Add(renderer);
            return renderer;
        }

        /// <summary>
        /// Destroy a renderer
        /// </summary>
        /// <param name="renderer">Renderer to destory</param>
        /// <exception cref="InvalidOperationException">If libass was not initialized beforehand</exception>
        /// <remarks>
        /// This method is a no-op if the renderer does not exist
        /// </remarks>
        public static void DestroyRenderer(Renderer renderer)
        {
            if (!Initialized) throw new InvalidOperationException("Libass was not initialized prior to renderer destruction");
            if (_renderers.Remove(renderer))
                renderer.Uninitialize();
        }

        /// <summary>
        /// Create a new track
        /// </summary>
        /// <returns>Track wrapper</returns>
        /// <exception cref="InvalidOperationException">If libass was not initialized beforehand</exception>
        public static unsafe Track CreateTrack()
        {
            if (!Initialized) throw new InvalidOperationException("Libass was not initialized prior to track creation");
            NativeTrack* ntrack = External.NewTrack(_library);
            Track track = new(ntrack);
            return track;
        }

        public static Track ReadFile()
        {
            return null;
        }

        public static unsafe Track ReadMemory(string buffer, string? codePage)
        {
            NativeTrack* ntrack = External.ReadMemory(_library, buffer, new UIntPtr((uint)buffer.Length), codePage);
            Track track = new(ntrack);
            return track;
        }

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
                    foreach (var renderer in _renderers)
                        renderer.Uninitialize();

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
