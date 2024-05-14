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
        public static bool Initialized => _initialized;

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

        public static int GetVersion()
        {
            if (!Initialized) return -1;
            return External.GetVersion();
        }
    }
}
