using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    public class Ffms2
    {
        private static bool _initialized;

        /// <summary>
        /// If the library is initialized
        /// </summary>
        public static bool Initialized => _initialized;

        /// <summary>
        /// Initialize the library
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void StartUp()
        {
            if (_initialized) return;

            try
            {
                External.Init(0, 0);
            } 
            catch (Exception e)
            {
                throw new Exception("An error occured while loading ffms2!", e);
            }

            _initialized = true;
        }

        /// <summary>
        /// Deinitialize the library
        /// </summary>
        /// <exception cref="Exception">An error occured during deinitialization</exception>
        public static void Shutdown()
        {
            if (!_initialized) return;

            try
            {
                External.Uninit();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to unitialize ffms2!", e);
            }
        }
        
        /// <summary>
        /// Get the integer representing a pixel format from its name
        /// </summary>
        /// <param name="name">Name of the format</param>
        /// <returns></returns>
        /// <exception cref="Exception">The pixel format was invalid</exception>
        public static int GetPixelFormat(string name)
        {
            try
            {
                return External.GetPixFmt(name);
            } catch (Exception e)
            {
                throw new Exception("The pixel format was invalid!", e);
            }
        }

        /// <summary>
        /// Get the FFMS2 version
        /// </summary>
        /// <returns></returns>
        public static int GetVersion()
        {
            if (!Initialized) return -1;
            return External.GetVersion();
        }

        /// <summary>
        /// Create a new ErrorInfo structure
        /// </summary>
        /// <returns></returns>
        internal static ErrorInfo NewErrorInfo()
        {
            return new ErrorInfo
            {
                BufferSize = 1024,
                Buffer = new string((char)0, 1024),
            };
        }
    }
}
