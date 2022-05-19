using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.AnimationSampling
{
    /// <summary>
    /// A class to handle sampling an <see cref="SkeletonAnimationTarget"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public class SkeletonAnimationTargetSampler
    {
        /// <summary>
        /// Gets the sampler that owns this.
        /// </summary>
        public SkeletonAnimationSampler Owner { get; private set; }

        /// <summary>
        /// Gets or Sets the bone this is targeting.
        /// </summary>
        public SkeletonBone Bone { get; private set; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public SkeletonAnimationTarget? Target { get; private set; }

        /// <summary>
        /// Gets the transform type.
        /// </summary>
        public TransformType TransformType { get; private set; }

        /// <summary>
        /// Gets or Sets the Translations Cursor
        /// </summary>
        private int CurrentTranslationsCursor { get; set; }

        /// <summary>
        /// Gets or Sets the Rotations Cursor
        /// </summary>
        private int CurrentRotationsCursor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="bone"></param>
        /// <param name="target"></param>
        public SkeletonAnimationTargetSampler(SkeletonAnimationSampler owner, SkeletonBone bone, SkeletonAnimationTarget? target, TransformType transformType)
        {
            Owner = owner;
            Bone = bone;
            Target = target;
            TransformType = transformType;
        }

        /// <summary>
        /// Updates the current animation bone sampler
        /// </summary>
        public void Update()
        {
            var translationAtFrame = TransformType == TransformType.Additive ? Bone.CurrentLocalTranslation : Bone.BaseLocalTranslation;
            var rotationAtFrame = TransformType == TransformType.Additive ? Bone.CurrentLocalRotation : Bone.BaseLocalRotation;

            if (Target != null && Bone.CanAnimate)
            {
                var bone = Target;
                var time = Owner.Owner.CurrentTime;

                var (firstRIndex, secondRIndex) = AnimationHelper.GetFramePairIndex(
                    bone.RotationFrames,
                    time,
                    0,
                    cursor: CurrentRotationsCursor);
                var (firstTIndex, secondTIndex) = AnimationHelper.GetFramePairIndex(
                    bone.TranslationFrames,
                    time,
                    0,
                    cursor: CurrentTranslationsCursor);

                // We have a rotation
                if (firstRIndex != -1)
                {
                    var firstFrame = bone.RotationFrames![firstRIndex];
                    var secondFrame = bone.RotationFrames![secondRIndex];

                    Quaternion rot;

                    // Identical Frames, no interpolating
                    if (firstRIndex == secondRIndex)
                        rot = bone.RotationFrames[firstRIndex].Value;
                    else
                        rot = Quaternion.Slerp(firstFrame.Value, secondFrame.Value, (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time));

                    Quaternion result = rotationAtFrame;

                    switch (TransformType)
                    {
                        case TransformType.Additive:
                            // Add to current frame
                            result *= rot;
                            break;
                        default:
                            // Take literal value
                            result = rot;
                            break;
                    }

                    // Blend between
                    rotationAtFrame = Quaternion.Slerp(rotationAtFrame, result, Owner.Owner.CurrentWeight);
                    // Update cursor (to speed up linear sampling if we're going forward)
                    CurrentRotationsCursor = firstRIndex;
                }

                if (firstTIndex != -1)
                {
                    var firstFrame = bone.TranslationFrames![firstTIndex];
                    var secondFrame = bone.TranslationFrames![secondTIndex];

                    Vector3 translation;

                    Vector3 result = translationAtFrame;

                    // Identical Frames, no interpolating
                    if (firstTIndex == secondTIndex)
                        translation = bone.TranslationFrames[firstTIndex].Value;
                    else
                        translation = Vector3.Lerp(firstFrame.Value, secondFrame.Value, (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time));

                    switch (TransformType)
                    {
                        case TransformType.Additive:
                            result += translation;
                            break;
                        case TransformType.Relative:
                            result = Bone.BaseLocalTranslation + translation;
                            break;
                        default:
                            // Take literal
                            result = translation;
                            break;
                    }

                    // Blend between
                    translationAtFrame = Vector3.Lerp(translationAtFrame, result, Owner.Owner.CurrentWeight);
                    // Update cursor (to speed up linear sampling if we're going forward)
                    CurrentTranslationsCursor = firstTIndex;
                }
            }

            Bone.CurrentLocalTranslation = translationAtFrame;
            Bone.CurrentLocalRotation = rotationAtFrame;
            Bone.GenerateCurrentWorldTransform();
        }
    }
}
