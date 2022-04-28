using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.AnimationSampling
{
    /// <summary>
    /// A class to handle sampling an <see cref="SkeletonAnimationTarget"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public class SkeletonAnimationTargetSamplerLayer
    {
        /// <summary>
        /// Gets or Sets the main layer that his belongs to.
        /// </summary>
        public AnimationSamplerLayer Layer { get; set; }

        /// <summary>
        /// Gets the underlying animation bone
        /// </summary>
        public SkeletonAnimationTarget Target { get; private set; }

        /// <summary>
        /// Gets or Sets the transform type.
        /// </summary>
        public TransformType TransformType { get; set; }

        /// <summary>
        /// Gets or Sets the Translations Cursor
        /// </summary>
        public int CurrentTranslationsCursor { get; set; }

        /// <summary>
        /// Gets or Sets the Rotations Cursor
        /// </summary>
        public int CurrentRotationsCursor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animBone"></param>
        public SkeletonAnimationTargetSamplerLayer(AnimationSamplerLayer layer, SkeletonAnimationTarget target, TransformType transformType)
        {
            Layer = layer;
            Target = target;
            TransformType = transformType;
        }
    }
}
