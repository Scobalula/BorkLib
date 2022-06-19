using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Compression.ZLIB
{
    internal unsafe class ZLIBInterop
    {
        internal enum MiniZReturnStatus
        {
            OK = 0,
            StreamEnd = 1,
            NeedDict = 2,
            ErrorNo = -1,
            StreamError = -2,
            DataError = -3,
            MemoryError = -4,
            BufferError = -5,
            VersionError = -6,
            ParamError = -10000
        };

        const string MiniZLibraryName = "MiniZ";

        [DllImport(MiniZLibraryName, EntryPoint = "mz_uncompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MZ_uncompress(byte* dest, ref int destLen, byte* source, int sourceLen, int windowBits);

        [DllImport(MiniZLibraryName, EntryPoint = "mz_deflateBound", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MZ_deflateBound(IntPtr stream, int inputSize);

        [DllImport(MiniZLibraryName, EntryPoint = "mz_compress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MZ_compress(byte* dest, ref int destLen, byte* source, int sourceLen);

        static ZLIBInterop()
        {
            NativeLibrary.SetDllImportResolver(typeof(ZLIBInterop).Assembly, NativeHelpers.DllImportResolver);
        }
    }
}
