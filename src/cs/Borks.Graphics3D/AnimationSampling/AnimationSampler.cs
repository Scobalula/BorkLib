using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.AnimationSampling
{
    /// <summary>
    /// A class to handle sampling an <see cref="Animation"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public class AnimationSampler
    {
        /// <summary>
        /// Gets or Sets the sampler layers
        /// </summary>
        public List<AnimationSamplerLayer> Layers { get; set; }

        // public AnimationSampler(Skeleton skeleton, Animation anim) : this(skeleton, new[] { ("Main", anim, 0.0f) }) { }

        public AnimationSampler(IEnumerable<(string, Animation, float)> anims, Skeleton? skeleton)
        {
            Layers = new();


        }
    }
}
