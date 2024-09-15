using System.Runtime.InteropServices;

namespace LibassCS
{
    internal static partial class External
    {
        [LibraryImport("libass", EntryPoint = "ass_library_init")]
        public static partial IntPtr Init();

        [LibraryImport("libass", EntryPoint = "ass_library_done")]
        public static partial void Uninit(IntPtr library);

        [LibraryImport("libass", EntryPoint = "ass_library_version")]
        public static partial int GetVersion();

        [LibraryImport("libass", EntryPoint = "ass_set_fonts_dir")]
        public static partial void SetFontsDir(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string path);

        [LibraryImport("libass", EntryPoint = "ass_set_extract_fonts")]
        public static partial void SetExtractFonts(IntPtr library, [MarshalAs(UnmanagedType.Bool)] bool extract);
    }
}
