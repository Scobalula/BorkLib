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
        /// Gets or Sets the translation frames.
        /// </summary>
        public List<AnimationFrame<Vector3>>? TranslationFrames { get; set; }

        /// <summary>
        /// Gets or Sets the rotation frames.
        /// </summary>
        public List<AnimationFrame<Quaternion>>? RotationFrames { get; set; }

        /// <summary>
        /// Gets or Sets the scale frames.
        /// </summary>
        public List<AnimationFrame<Vector3>>? ScaleFrames { get; set; }

        /// <summary>
        /// Gets or Sets the transform type for this bone.
        /// </summary>
        public TransformType TransformType { get; set; }

        /// <summary>
        /// Gets the number of translations frames.
        /// </summary>
        public int TranslationFrameCount => TranslationFrames != null ? TranslationFrames.Count : 0;

        /// <summary>
        /// Gets the number of rotation frames.
        /// </summary>
        public int RotationFrameCount => RotationFrames != null ? RotationFrames.Count : 0;

        /// <summary>
        /// Gets the number of scale frames.
        /// </summary>
        public int ScaleFrameCount => ScaleFrames != null ? ScaleFrames.Count : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletonAnimationTarget"/> class with the provided data.
        /// </summary>
        /// <param name="boneName">Name of the bone that we are targeting.</param>
        public SkeletonAnimationTarget(string boneName)
        {
            BoneName = boneName;
            TransformType = TransformType.Parent;
        }

        public Vector3 SampleTranslation(float time)
        {
            var sample = Vector3.Zero;

            var (i0, i1) = AnimationHelper.GetFramePairIndex(TranslationFrames, time, 0.0f);

            if(i0 != -1 && i1 != -1)
            {
                var firstFrame = TranslationFrames![i0];
                var secondFrame = TranslationFrames![i1];

                if (i0 == i1)
                    sample = firstFrame.Value;
                else
                    sample = Vector3.Lerp(firstFrame.Value, secondFrame.Value, (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time));
            }

            return sample;
        }

        public Quaternion SampleRotation(float time)
        {
            var sample = Quaternion.Identity;

            var (i0, i1) = AnimationHelper.GetFramePairIndex(RotationFrames, time, 0.0f);

            if (i0 != -1 && i1 != -1)
            {
                var firstFrame = RotationFrames![i0];
                var secondFrame = RotationFrames![i1];

                if (i0 == i1)
                    sample = firstFrame.Value;
                else
                    sample = Quaternion.Slerp(firstFrame.Value, secondFrame.Value, (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time));
            }

            return sample;
        }
    }
}
