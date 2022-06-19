using System.Runtime.InteropServices;

namespace Borks.Compression.LZ4
{
    internal unsafe class LZ4Interop
    {
        const string LZ4LibraryName = "LZ4";

        [DllImport(LZ4LibraryName, EntryPoint = "LZ4_decompress_safe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_decompress_safe(byte* src, byte* dst, int compressedSize, int dstCapacity);

        [DllImport(LZ4LibraryName, EntryPoint = "LZ4_compressBound", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_compressBound(int inputSize);

        [DllImport(LZ4LibraryName, EntryPoint = "LZ4_compress_default", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_compress_default(byte* src, byte* dst, int compressedSize, int dstCapacity);

        static LZ4Interop()
        {
            NativeLibrary.SetDllImportResolver(typeof(LZ4Interop).Assembly, NativeHelpers.DllImportResolver);
        }
    }
}