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
    }
}
