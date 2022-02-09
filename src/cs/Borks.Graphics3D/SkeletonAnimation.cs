using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class that holds animation data for a <see cref="Skeleton"/>.
    /// </summary>
    public class SkeletonAnimation
    {
        /// <summary>
        /// Gets or Sets the name of the skeleton animation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the skeleton tied to this animation, if any.
        /// </summary>
        public Skeleton? Skeleton { get; set; }

        /// <summary>
        /// Gets or Sets the targets that contain animation frames.
        /// </summary>
        public List<SkeletonAnimationTarget> Targets { get; set; }


        public SkeletonAnimation(string name)
        {
            Name = name;
            Targets = new();
        }
    }
}
