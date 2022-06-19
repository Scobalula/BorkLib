using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Borks
{
    /// <summary>
    /// Represents a Pattern with a Needle and a Mask for use in pattern matching and search/find methods
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    public struct Pattern<T>
    {
        /// <summary>
        /// Gets or Sets the Needle
        /// </summary>
        public T[] Needle { get; set; }

        /// <summary>
        /// Gets or Sets the Mask
        /// </summary>
        public T[] Mask { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Pattern{T}"/> of the given type.
        /// </summary>
        /// <param name="needle">The needle to search for.</param>
        /// <param name="mask">The mask to use for unknown values.</param>
        public Pattern(T[] needle, T[] mask)
        {
            Needle = needle;
            Mask = mask;
        }
    }
}
