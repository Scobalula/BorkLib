using System.Numerics;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class that holds a data stored within a <see cref="SkeletonAnimation"/> instance that transforms the target.
    /// </summary>
    public class SkeletonAnimationTarget
    {
        /// <summary>
        /// Gets or Sets the name of the bone this channel targets
        /// </summary>
        public string BoneName { get; set; }

        /// <summary>
        /// Gets or Sets the Translation Frames
        /// </summary>
        public List<AnimationFrame<Vector3>>? TranslationFrames { get; set; }

        /// <summary>
        /// Gets or Sets the Rotation Frames
        /// </summary>
        public List<AnimationFrame<Quaternion>>? RotationFrames { get; set; }

        /// <summary>
        /// Gets or Sets the Translation Frames
        /// </summary>
        public List<AnimationFrame<Vector3>>? ScaleFrames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletonAnimationTarget"/> class with the provided data.
        /// </summary>
        /// <param name="boneName">Name of the bone that we are targeting.</param>
        public SkeletonAnimationTarget(string boneName)
        {
            BoneName = boneName;
        }
    }
}
