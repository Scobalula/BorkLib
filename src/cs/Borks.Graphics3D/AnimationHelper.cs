using Borks.Graphics3D.AnimationSampling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    public class AnimationHelper
    {
        /// <summary>
        /// Gets the frame pair indices for the given animation frame list
        /// </summary>
        public static (int, int) GetFramePairIndex<T>(List<AnimationFrame<T>>? list, float time, float startTime, float minTime = float.MinValue, float maxTime = float.MaxValue, int cursor = 0)
        {
            // Early quit for lists that we can't "pair"
            if (list == null)
                return (-1, -1);
            if (list.Count == 0)
                return (-1, -1);
            if (list.Count == 1)
                return (0, 0);
            if (time > (startTime + list.Last().Time))
                return (list.Count - 1, list.Count - 1);
            if (time < (startTime + list.First().Time))
                return (0, 0);

            int i;

            // First pass from cursor
            for (i = 0; i < list.Count - 1; i++)
            {
                if (time < (startTime + list[i + 1].Time))
                    return (i, i + 1);
            }

            // Second pass up to cursor
            for (i = 0; i < list.Count - 1 && i < cursor; i++)
            {
                if (time < (startTime + list[i + 1].Time))
                    return (i, i + 1);
            }

            return (list.Count - 1, list.Count - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetWeight(List<AnimationFrame<float>> weights, float time, float startTime, float defaultWeight, ref int cursor)
        {
            var (firstIndex, secondIndex) = GetFramePairIndex(weights, time, startTime, cursor:cursor);
            var result = defaultWeight;

            if (firstIndex != -1)
            {
                if (firstIndex == secondIndex)
                {
                    result = weights[firstIndex].Value;
                }
                else
                {
                    var firstFrame = weights[firstIndex];
                    var secondFrame = weights[secondIndex];

                    var lerpAmount = (time - (startTime + firstFrame.Time)) / ((startTime + secondFrame.Time) - (startTime + firstFrame.Time));

                    result = (firstFrame.Value * (1 - lerpAmount)) + (secondFrame.Value * lerpAmount);
                }

                cursor = firstIndex;
            }

            return result;
        }

        /// <summary>
        /// Appends the provided animation onto the end of the root animation.
        /// </summary>
        /// <param name="root">The animation that the input animation is being appened onto.</param>
        /// <param name="input">The animation being appened.</param>
        public static void Append(Animation? root, Animation? input)
        {
            if (root == null || input == null)
                return;

            var start = root.GetAnimationFrameCount();

            // Merge skeleton animation
            if(input.SkeletonAnimation != null)
            {
                if (root.SkeletonAnimation == null)
                    root.SkeletonAnimation = new(input.SkeletonAnimation.Skeleton);

                var rootSkelAnim = root.SkeletonAnimation;

                foreach (var target in input.SkeletonAnimation.Targets)
                {
                    var newTarget = rootSkelAnim.CreateTarget(target.BoneName);

                    if(target.TranslationFrames is not null)
                    {
                        newTarget.TranslationFrames ??= new(target.TranslationFrames.Count);

                        foreach (var frame in target.TranslationFrames)
                            newTarget.TranslationFrames.Add(new(start + frame.Time, frame.Value));
                    }
                    if (target.RotationFrames is not null)
                    {
                        newTarget.RotationFrames ??= new(target.RotationFrames.Count);

                        foreach (var frame in target.RotationFrames)
                            newTarget.RotationFrames.Add(new(start + frame.Time, frame.Value));
                    }
                    if (target.ScaleFrames is not null)
                    {
                        newTarget.ScaleFrames ??= new(target.ScaleFrames.Count);

                        foreach (var frame in target.ScaleFrames)
                            newTarget.ScaleFrames.Add(new(start + frame.Time, frame.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Converts the provided <see cref="Animation"/> that contains a <see cref="SkeletonAnimation"/> to the given transform space.
        /// </summary>
        /// <param name="input">The input <see cref="Animation"/>. If the provided input has no skeleton animation, then this is returned.</param>
        /// <param name="newSpace">The new <see cref="TransformSpace"/> to convert the animation to.</param>
        /// <returns>Resulting animation, if no skeleton animation is provided or the space is the same, the input animation is returned.</returns>
        public static Animation ConvertTransformSpace(Animation input, TransformSpace newSpace) =>
            ConvertTransformSpace(input, newSpace);

        /// <summary>
        /// Converts the provided <see cref="Animation"/> that contains a <see cref="SkeletonAnimation"/> to the given transform space.
        /// </summary>
        /// <param name="input">The input <see cref="Animation"/>. If the provided input has no skeleton animation, then this is returned.</param>
        /// <param name="skeleton">The <see cref="Skeleton"/> to use for sampling, if null, it is taken from the <see cref="SkeletonAnimation"/>.</param>
        /// <param name="newSpace">The new <see cref="TransformSpace"/> to convert the animation to.</param>
        /// <returns>Resulting animation, if no skeleton animation is provided or the space is the same, the input animation is returned.</returns>
        /// <exception cref="ArgumentNullException">Thrown if no skeleton is provided.</exception>
        public static Animation ConvertTransformSpace(Animation input, Skeleton? skeleton, TransformSpace newSpace)
        {
            // If no skeleton animation is provided, then we can do early quit, and return this.
            if (input.SkeletonAnimation == null)
                return input;
            // Early quit for same space.
            if (input.SkeletonAnimation.TransformSpace == newSpace)
                return input;
            // We require a skeleton to sample from.
            if (skeleton == null)
                skeleton = input.SkeletonAnimation.Skeleton;
            if (skeleton == null)
                throw new ArgumentNullException(nameof(skeleton));

            var newAnim = new Animation(skeleton);
            var newSkelAnim = newAnim.SkeletonAnimation!;
            var sampler = new AnimationSampler(input, skeleton);

            // Set up, our positions are going to be absolute
            newSkelAnim.TransformSpace = newSpace;
            newSkelAnim.TransformType = TransformType.Absolute;

            // Loop each animation and add to our current skeleton.
            for (int i = 0; i < sampler.FrameCount; i++)
            {
                sampler.Update(i, AnimationSampleType.AbsoluteFrameTime);

                foreach (var target in sampler.SkeletonAnimationSampler!.TargetSamplers)
                {
                    if(target.Value.Target != null)
                    {
                        var newTarget = newSkelAnim.CreateTarget(target.Value.Bone.Name);

                        if(newSpace == TransformSpace.Local)
                        {
                            newTarget.AddTranslationFrame(i, target.Value.Bone.CurrentLocalTranslation);
                            newTarget.AddRotationFrame(i, target.Value.Bone.CurrentLocalRotation);
                        }
                        else
                        {
                            newTarget.AddTranslationFrame(i, target.Value.Bone.CurrentWorldTranslation);
                            newTarget.AddRotationFrame(i, target.Value.Bone.CurrentWorldRotation);
                        }
                    }
                }
            }

            return newAnim;
        }
    }
}
