using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.AnimationSampling
{
    /// <summary>
    /// An interface that describes an animation solver for use during sampling.
    /// </summary>
    public interface IAnimationSamplerSolver
    {
        /// <summary>
        /// Updates this solver at the given time.
        /// </summary>
        /// <param name="time">Absolute frame time to update the solver at.</param>
        void Update(float time);
    }
}
