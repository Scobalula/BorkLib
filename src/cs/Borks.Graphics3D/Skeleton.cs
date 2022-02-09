using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class that defines 
    /// </summary>
    public class Skeleton
    {
        /// <summary>
        /// Gets or Sets the bones stored within this skeleton.
        /// </summary>
        public List<SkeletonBone> Bones { get; set; }

        public Skeleton()
        {
            Bones = new();
        }
    }
}
