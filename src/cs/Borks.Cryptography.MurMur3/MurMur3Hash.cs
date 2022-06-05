using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Cryptography.MurMur3
{
    /// <summary>
    /// A class that computes the <see cref="MurMur3Hash"/> for the given input.
    /// </summary>
    public class MurMur3Hash : HashAlgorithm
    {
        /// <summary>
        /// Gets the current hash value.
        /// </summary>
        public uint Value { get; private set; }

        /// <summary>
        /// Gets or Sets the seed.
        /// </summary>
        private uint Seed { get; set; }

        /// <summary>
        /// Gets or Sets the current length. This is required for <see cref="HashCore(byte[], int, int)"/>.
        /// </summary>
        private int Length { get; set; }

        /// <summary>
        /// Gets or Sets the current tail value.
        /// </summary>
        private uint Tail { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MurMur3Hash"/> class with a seed of 0.
        /// </summary>
        public MurMur3Hash()
        {
            Seed = 0;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MurMur3Hash"/> class with a given seed.
        /// </summary>
        /// <param name="seed">The seed to initialize the value at.</param>
        public MurMur3Hash(uint seed)
        {
            Seed = seed;
            Initialize();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Value = Seed;
            HashSizeValue = 32;
            Length = 0;
        }

        /// <inheritdoc/>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            Length += cbSize;
            Value = MurMur3.CalculateBlock32(array, Value);
        }

        /// <inheritdoc/>
        protected override byte[] HashFinal()
        {
            Value ^= (uint)Length;

            Value ^= Value >> 16;
            Value *= 0x85ebca6b;
            Value ^= Value >> 13;
            Value *= 0xc2b2ae35;
            Value ^= Value >> 16;

            return BitConverter.GetBytes(Value);
        }
    }
}
