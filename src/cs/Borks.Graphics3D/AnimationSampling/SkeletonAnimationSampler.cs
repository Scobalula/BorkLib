using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.AnimationSampling
{
    /// <summary>
    /// A class to handle sampling an <see cref="SkeletonAnimation"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public class SkeletonAnimationSampler
    {
        /// <summary>
        /// Gets the sampler that owns this skeleton animation sampler.
        /// </summary>
        public AnimationSampler Owner { get; private set; }

        /// <summary>
        /// Gets or Sets the animation that this sampler is sampling from.
        /// </summary>
        public SkeletonAnimation Animation { get; set; }

        /// <summary>
        /// Gets or Sets the skeleton that this sampler is targeting.
        /// </summary>
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Gets or Sets the targets.
        /// </summary>
        public Dictionary<string, SkeletonAnimationTargetSampler> TargetSamplers { get; set; }


        public SkeletonAnimationSampler(AnimationSampler owner, SkeletonAnimation skeletonAnimation, Skeleton skeleton)
        {
            Owner = owner;
            Skeleton = skeleton;
            Animation = skeletonAnimation;
            TargetSamplers = new();

            Skeleton.InitializeAnimationTransforms();

            foreach(var bone in skeleton.EnumerateBones())
            {
                // Attempt to find a target
                var targetIndex = Animation.Targets.FindIndex(x =>
                {
                    return x.BoneName.Equals(
                        bone.Name, 
                        StringComparison.CurrentCultureIgnoreCase);
                });

                SkeletonAnimationTarget? target = null;

                if (targetIndex != -1)
                    target = Animation.Targets[targetIndex];

                TargetSamplers.Add(bone.Name, new(
                    this,
                    bone,
                    target,
                    skeletonAnimation.TransformType,
                    skeletonAnimation.TransformSpace));
            }
        }

        public void SetTransformType(TransformType type)
        {
            foreach (var targetSampler in TargetSamplers)
            {
                targetSampler.Value.TransformType = type;
            }
        }

        public void Update()
        {
            
            foreach (var targetSampler in TargetSamplers)
            {
                targetSampler.Value.Update();
            }
        }
    }
}
