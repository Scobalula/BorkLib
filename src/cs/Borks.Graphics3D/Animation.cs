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
        /// Gets or Sets the animation frame rate.
        /// </summary>
        public float Framerate { get; set; }

        /// <summary>
        /// Gets the number of skeletal animation targets.
        /// </summary>
        public int SkeletalTargetCount => SkeletonAnimation != null ? SkeletonAnimation.Targets.Count : 0;

        /// <summary>
        /// Gets the skeletal animation transform type.
        /// </summary>
        public TransformType SkeletalTransformType => SkeletonAnimation != null ? SkeletonAnimation.TransformType : TransformType.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="Animation"/> class.
        /// </summary>
        /// <param name="skeleton">Skeleton to assign to the instance of the <see cref="SkeletonAnimation"/>.</param>
        public Animation(Skeleton skeleton)
        {
            Framerate = 30.0f;
            SkeletonAnimation = new(skeleton);
        }

        /// <summary>
        /// Calculates the frame count based off the animation frame ranges.
        /// </summary>
        /// <returns>The total frame count based off the frame range.</returns>
        public float GetAnimationFrameCount()
        {
            var minFrame = float.MaxValue;
            var maxFrame = float.MinValue;

            if(SkeletonAnimation != null)
            {
                foreach (var target in SkeletonAnimation.Targets)
                {
                    if (target.RotationFrames != null)
                    {
                        foreach (var f in target.RotationFrames)
                        {
                            minFrame = MathF.Min(minFrame, f.Time);
                            maxFrame = MathF.Max(maxFrame, f.Time);
                        }
                    }

                    if (target.TranslationFrames != null)
                    {
                        foreach (var f in target.TranslationFrames)
                        {
                            minFrame = MathF.Min(minFrame, f.Time);
                            maxFrame = MathF.Max(maxFrame, f.Time);
                        }
                    }

                    if (target.ScaleFrames != null)
                    {
                        foreach (var f in target.ScaleFrames)
                        {
                            minFrame = MathF.Min(minFrame, f.Time);
                            maxFrame = MathF.Max(maxFrame, f.Time);
                        }
                    }
                }
            }

            return maxFrame - minFrame;
        }

        /// <summary>
        /// Checks whether or not any of the skeletal targets has translation frames.
        /// </summary>
        /// <returns>True if any of the targets has frames, otherwise false.</returns>
        public bool HasSkeletalTranslationFrames()
        {
            if(SkeletonAnimation != null)
            {
                foreach (var target in SkeletonAnimation.Targets)
                {
                    if(target.TranslationFrames == null || target.TranslationFrames.Count <= 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether or not any of the skeletal targets has rotation frames.
        /// </summary>
        /// <returns>True if any of the targets has frames, otherwise false.</returns>
        public bool HasSkeletalRotationFrames()
        {
            if (SkeletonAnimation != null)
            {
                foreach (var target in SkeletonAnimation.Targets)
                {
                    if (target.RotationFrames == null || target.RotationFrames.Count <= 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether or not any of the skeletal targets has scales frames.
        /// </summary>
        /// <returns>True if any of the targets has frames, otherwise false.</returns>
        public bool HasSkeletalScalesFrames()
        {
            if (SkeletonAnimation != null)
            {
                foreach (var target in SkeletonAnimation.Targets)
                {
                    if (target.ScaleFrames == null || target.ScaleFrames.Count <= 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
