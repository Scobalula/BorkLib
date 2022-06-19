using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Compression.Oodle
{
    /// <summary>
    /// Provides methods for interacting with Oodle's native library.
    /// </summary>
    public unsafe class OodleInterop
    {
        internal static IntPtr OodleHandle;

        internal static delegate* unmanaged[Cdecl]<byte*, int, byte*, int, int, int, int, byte*, int, long, long, byte*, int, int, long> OodleDecompress;

        /// <summary>
        /// Sets the library to use for working with Oodle.
        /// </summary>
        /// <param name="path">The path of the library to use.</param>
        public static void SetOodleLibrary(string path)
        {
            // Since Oodle is a closed source lib and there are multiple versions
            // we must dynamically allow defining the exact DLL being loaded for it.
            // For other compression libs we have control over building their source.
            FreeOodleLibrary();
            OodleHandle = NativeLibrary.Load(path);
            OodleDecompress = (delegate* unmanaged[Cdecl]<byte*, int, byte*, int, int, int, int, byte*, int, long, long, byte*, int, int, long>)NativeLibrary.GetExport(OodleHandle, "OodleLZ_Decompress");
        }

        /// <summary>
        /// Frees the library used for working with Oodle.
        /// </summary>
        public static void FreeOodleLibrary()
        {
            NativeLibrary.Free(OodleHandle);
        }
    }
}
