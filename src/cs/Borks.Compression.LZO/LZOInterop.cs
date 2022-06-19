using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Compression.LZO
{
    internal unsafe class LZOInterop
    {
        internal enum LZOkayResult
        {
            LookbehindOverrun = -4,
            OutputOverrun = -3,
            InputOverrun = -2,
            Error = -1,
            Success = 0,
            InputNotConsumed = 1,
        }

        const string LZOLibraryName = "LZOkay";

        [DllImport(LZOLibraryName, EntryPoint = "LZOkayDecompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZOkayDecompress(byte* dest, ref int destLen, byte* source, int sourceLen);

        [DllImport(LZOLibraryName, EntryPoint = "LZOkayCompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZOkayCompress(byte* dest, ref int destLen, byte* source, int sourceLen);

        static LZOInterop()
        {
            NativeLibrary.SetDllImportResolver(typeof(LZOInterop).Assembly, NativeHelpers.DllImportResolver);
        }
    }
}
