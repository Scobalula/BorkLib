using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Cryptography.MurMur3
{
    public class MurMur3
    {
        public static uint Calculate32(byte[] buffer, uint seed)
        {
            var result = CalculateBlock32(buffer, seed);
            return CalculateFinal32(result, buffer.Length);
        }

        public static uint CalculateBlock32(byte[] buffer, uint seed)
        {
            var b = MemoryMarshal.Cast<byte, uint>(buffer);

            uint h1 = seed;
            uint c1 = 0xcc9e2d51;
            uint c2 = 0x1b873593;

            for (int i = 0; i < b.Length; i++)
            {
                uint k1 = b[i];

                k1 *= c1;
                k1 = BitOperations.RotateLeft(k1, 15);
                k1 *= c2;

                h1 ^= k1;
                h1 = BitOperations.RotateLeft(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }

            uint final = 0;
            var tail = buffer.Length & 3;
            var tailIndex = b.Length * 4;

            if (tail >= 3)
            {
                final ^= (uint)(buffer[tailIndex + 2] << 16);
            }
            if (tail >= 2)
            {
                final ^= (uint)(buffer[tailIndex + 1] << 8);
            }
            if (tail >= 1)
            {
                final ^= buffer[tailIndex + 0];
                final *= c1;
                final = BitOperations.RotateLeft(final, 15);
                final *= c2;
                h1 ^= final;
            }

            return h1;
        }

        public static uint CalculateFinal32(uint h1, int length)
        {
            h1 ^= (uint)length;

            h1 ^= h1 >> 16;
            h1 *= 0x85ebca6b;
            h1 ^= h1 >> 13;
            h1 *= 0xc2b2ae35;
            h1 ^= h1 >> 16;

            return h1;
        }

        public static uint Calculate32(byte[] buffer)
        {
            var b = MemoryMarshal.Cast<byte, uint>(buffer);

            Console.WriteLine(buffer.Length);
            Console.WriteLine(b.Length);

            uint h1 = 0;
            uint c1 = 0xcc9e2d51;
            uint c2 = 0x1b873593;

            for (int i = 0; i < b.Length; i++)
            {
                uint k1 = b[i];

                k1 *= c1;
                k1 = BitOperations.RotateLeft(k1, 15);
                k1 *= c2;

                h1 ^= k1;
                h1 = BitOperations.RotateLeft(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }

            uint final = 0;

            var tail = buffer.Length & 3;
            var tailIndex = b.Length * 4;

            if (tail >= 3)
            {
                final ^= (uint)(buffer[tailIndex + 2] << 16);
            }
            if (tail >= 2)
            {
                final ^= (uint)(buffer[tailIndex + 1] << 8);
            }
            if (tail >= 1)
            {
                final ^= buffer[tailIndex + 0];
                final *= c1;
                final = BitOperations.RotateLeft(final, 15);
                final *= c2;
                h1 ^= final;
            }


            //----------
            // finalization

            h1 ^= (uint)buffer.Length;

            h1 ^= h1 >> 16;
            h1 *= 0x85ebca6b;
            h1 ^= h1 >> 13;
            h1 *= 0xc2b2ae35;
            h1 ^= h1 >> 16;

            return h1;
        }
    }
}
