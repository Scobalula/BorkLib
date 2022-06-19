
namespace Borks.Compression.Oodle
{
    /// <summary>
    /// Methods to work with Oodle data, requires Oodle DLL/SDK
    /// </summary>
    public unsafe static class Oodle
    {
        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="outputSize">Size of the data</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Decompress(byte[] inputBuffer, int outputSize) =>
            Decompress(inputBuffer, outputSize, OodleFuzzSafe.No, OodleCheckCRC.No, OodleVerbosity.No);

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="outputSize">Size of the data</param>
        /// <param name="fuzzSafe"><see cref="OodleFuzzSafe"/> value</param>
        /// <param name="checkCrc"><see cref="OodleCheckCRC"/> value</param>
        /// <param name="verbosity"><see cref="OodleVerbosity"/> value</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Decompress(byte[] inputBuffer, int outputSize, OodleFuzzSafe fuzzSafe, OodleCheckCRC checkCrc, OodleVerbosity verbosity)
        {
            var rawBuf = new byte[outputSize];

            fixed (byte* a = &inputBuffer[0])
            {
                fixed (byte* b = &rawBuf[0])
                {
                    var result = OodleInterop.OodleDecompress(
                        a,
                        inputBuffer.Length,
                        b,
                        rawBuf.Length,
                        (int)fuzzSafe,
                        (int)checkCrc,
                        (int)verbosity,
                        null, 0,
                        0, 0,
                        null, 0,
                        (int)OodleThreading.None);
                }
            }

            return rawBuf;
        }

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="inputOffset">Offset within the buffer of the data</param>
        /// <param name="inputSize">Size of the data</param>
        /// <param name="outputBuffer">Buffer to output to</param>
        /// <param name="outputOffset">Offset within the buffer to place the data</param>
        /// <param name="outputSize">Size of the data</param>
        public static int Decompress(byte[] inputBuffer, int inputOffset, int inputSize, byte[] outputBuffer, int outputOffset, int outputSize) =>
            Decompress(inputBuffer, inputOffset, inputSize, outputBuffer, outputOffset, outputSize, OodleFuzzSafe.No, OodleCheckCRC.No, OodleVerbosity.No);

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="inputOffset">Offset within the buffer of the data</param>
        /// <param name="inputSize">Size of the data</param>
        /// <param name="outputBuffer">Buffer to output to</param>
        /// <param name="outputOffset">Offset within the buffer to place the data</param>
        /// <param name="outputSize">Size of the data</param>
        /// <param name="fuzzSafe"><see cref="OodleFuzzSafe"/> value</param>
        /// <param name="checkCrc"><see cref="OodleCheckCRC"/> value</param>
        /// <param name="verbosity"><see cref="OodleVerbosity"/> value</param>
        /// <returns>The total number of bytes placed into the buffer.
        /// This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available,
        /// or zero (0) if the end of the data has been reached.</returns>
        public static int Decompress(byte[] inputBuffer, int inputOffset, int inputSize, byte[] outputBuffer, int outputOffset, int outputSize, OodleFuzzSafe fuzzSafe, OodleCheckCRC checkCrc, OodleVerbosity verbosity)
        {
            if (inputBuffer is null)
                throw new ArgumentNullException(nameof(inputBuffer), "Input Buffer cannot be null");
            if (inputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(inputOffset), "Input offset must be a non-negative number");
            if ((uint)inputSize > inputBuffer.Length - inputOffset)
                throw new ArgumentOutOfRangeException(nameof(inputSize), "Input size is outside the bounds of the buffer");

            if (outputBuffer is null)
                throw new ArgumentNullException(nameof(outputBuffer), "Output Buffer cannot be null");
            if (outputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(outputOffset), "Output offset must be a non-negative number");
            if (outputSize < 0 || outputSize > outputBuffer.Length - outputOffset)
                throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size is outside the bounds of the buffer");

            fixed (byte* a = &inputBuffer[0])
            {
                fixed (byte* b = &outputBuffer[0])
                {
                    return (int)OodleInterop.OodleDecompress(
                        a + inputOffset,
                        inputSize,
                        b + outputOffset,
                        outputSize,
                        (int)fuzzSafe,
                        (int)checkCrc,
                        (int)verbosity,
                        null, 0,
                        0,
                        0,
                        null, 0,
                        (int)OodleThreading.None);
                }
            }
        }
    }
}
