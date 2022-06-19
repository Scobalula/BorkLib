using System.Runtime.InteropServices;

namespace Borks.Compression.ZStandard
{
    internal unsafe class ZStandardInterop
    {
        #region ZStandard
        const string ZStdLibraryName = "ZStandard";

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_decompress", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ZSTD_decompress(void* dst, int dstCapacity, void* src, int srcCapacity);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_compress", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ZSTD_compress(void* dst, int dstCapacity, void* src, int srcCapacity, int compressionLevel);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_getFrameContentSize")]
        internal static extern int ZSTD_getFrameContentSize(void* src, int srcSize);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        internal static extern sbyte* ZSTD_getErrorName(int errorCode);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_compressBound")]
        internal static extern int ZSTD_compressBound(int srcSize);
        #endregion

        static ZStandardInterop()
        {
            NativeLibrary.SetDllImportResolver(typeof(ZStandardInterop).Assembly, NativeHelpers.DllImportResolver);
        }
    }
}