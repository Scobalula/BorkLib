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
        /// Gets or Sets the skeleton tied to this animation, if any.
        /// </summary>
        public Skeleton? Skeleton { get; set; }

        /// <summary>
        /// Gets or Sets the targets that contain animation frames.
        /// </summary>
        public List<SkeletonAnimationTarget> Targets { get; set; }

        /// <summary>
        /// Gets or Sets the transform type.
        /// </summary>
        public TransformType TransformType { get; set; }

        public SkeletonAnimation()
        {
            Targets = new();
            TransformType = TransformType.Unknown;
        }

        public SkeletonAnimation(Skeleton skeleton)
        {
            Skeleton = skeleton;
            Targets = new();
        }

        public SkeletonAnimation(Skeleton? skeleton, int targetCount, TransformType type)
        {
            Skeleton = skeleton;
            Targets = new(targetCount);
            TransformType = type;
        }

        /// <summary>
        /// Creates a new instance of an <see cref="SkeletonAnimationTarget"/> within this animation, if one already exists with this name, then that target is returned.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <returns>A new target that is added to this animation if it doesn't exist, otherwise an existing target with the given name.</returns>
        public SkeletonAnimationTarget CreateTarget(string name)
        {
            var idx = Targets.FindIndex(x => x.BoneName == name);

            if (idx != -1)
                return Targets[idx];

            var nTarget = new SkeletonAnimationTarget(name);
            Targets.Add(nTarget);

            return nTarget;
        }
    }
}
