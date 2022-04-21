using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold an animation frame.
    /// </summary>
    /// <typeparam name="T">Data type stored within this frame.</typeparam>
    public struct AnimationFrame<T>
    {
        /// <summary>
        /// Gets or Sets the time this frame occurs at.
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// Gets or Sets the 
        /// </summary>
        public T Value { get; set; }

        public AnimationFrame(float time, T value)
        {
            Time = time;
            Value = value;
        }
    }
}
