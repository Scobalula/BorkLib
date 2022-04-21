using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a 3D Animation that animations skeletons and/or other data.
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// Gets or Sets the skeleton animation stored within this animation.
        /// </summary>
        public SkeletonAnimation? SkeletonAnimation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Animation"/> class.
        /// </summary>
        /// <param name="skeleton">Skeleton to assign to the instance of the <see cref="SkeletonAnimation"/>.</param>
        public Animation(Skeleton skeleton)
        {
            SkeletonAnimation = new(skeleton);
        }

        public int GetAnimationFrameCount()
        {
            var total = 0;

            return total;
        }
    }
}
