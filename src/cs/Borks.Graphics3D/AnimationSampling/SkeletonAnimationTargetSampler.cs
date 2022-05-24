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
        public TransformType TransformType { get; internal set; }

        /// <summary>
        /// Gets the transform space.
        /// </summary>
        public TransformSpace TransformSpace { get; internal set; }

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
        public SkeletonAnimationTargetSampler(SkeletonAnimationSampler owner, SkeletonBone bone, SkeletonAnimationTarget? target, TransformType transformType, TransformSpace transformSpace)
        {
            Owner = owner;
            Bone = bone;
            Target = target;
            TransformType = transformType;
            TransformSpace = transformSpace;
        }

        /// <summary>
        /// Updates the current animation bone sampler
        /// </summary>
        public void Update()
        {
            var isLocal = TransformSpace == TransformSpace.Local;

            if (Target != null && Bone.CanAnimate)
            {
                var bone = Target;
                var time = Owner.Owner.CurrentTime;

                var (firstRIndex, secondRIndex) = AnimationHelper.GetFramePairIndex(
                    bone.RotationFrames,
                    time,
                    Owner.Owner.StartFrame,
                    cursor: CurrentRotationsCursor);
                var (firstTIndex, secondTIndex) = AnimationHelper.GetFramePairIndex(
                    bone.TranslationFrames,
                    time,
                    Owner.Owner.StartFrame,
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
                        rot = Quaternion.Slerp(firstFrame.Value, secondFrame.Value, (time - (Owner.Owner.StartFrame + firstFrame.Time)) / ((Owner.Owner.StartFrame + secondFrame.Time) - (Owner.Owner.StartFrame + firstFrame.Time)));

                    Quaternion result = TransformType switch
                    {
                        TransformType.Additive => Bone.CurrentLocalRotation * rot,
                        _                      => rot,
                    };

                    // Blend between
                    if(isLocal)
                        Bone.CurrentLocalRotation = Quaternion.Slerp(Bone.CurrentLocalRotation, result, Owner.Owner.CurrentWeight);
                    else
                        Bone.CurrentWorldRotation = Quaternion.Slerp(Bone.CurrentWorldRotation, result, Owner.Owner.CurrentWeight);
                    // Update cursor (to speed up linear sampling if we're going forward)
                    CurrentRotationsCursor = firstRIndex;
                }

                if (firstTIndex != -1)
                {
                    var firstFrame = bone.TranslationFrames![firstTIndex];
                    var secondFrame = bone.TranslationFrames![secondTIndex];

                    Vector3 translation;

                    // Identical Frames, no interpolating
                    if (firstTIndex == secondTIndex)
                        translation = bone.TranslationFrames[firstTIndex].Value;
                    else
                        translation = Vector3.Lerp(firstFrame.Value, secondFrame.Value, (time - (Owner.Owner.StartFrame + firstFrame.Time)) / ((Owner.Owner.StartFrame + secondFrame.Time) - (Owner.Owner.StartFrame + firstFrame.Time)));
                    
                    var result = TransformType switch
                    {
                        TransformType.Additive => Bone.CurrentLocalTranslation + translation,
                        TransformType.Relative => Bone.BaseLocalTranslation + translation,
                        _                      => translation,
                    };

                    // Blend between
                    if(isLocal)
                        Bone.CurrentLocalTranslation = Vector3.Lerp(Bone.CurrentLocalTranslation, result, Owner.Owner.CurrentWeight);
                    else
                        Bone.CurrentWorldTranslation = Vector3.Lerp(Bone.CurrentWorldTranslation, result, Owner.Owner.CurrentWeight);
                    // Update cursor (to speed up linear sampling if we're going forward)
                    CurrentTranslationsCursor = firstTIndex;
                }
            }

            if (isLocal)
                Bone.GenerateCurrentWorldTransform();
            else
                Bone.GenerateCurrentLocalTransform();
        }
    }
}
