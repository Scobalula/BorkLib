﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.AnimationSampling
{
    /// <summary>
    /// A class to handle sampling an <see cref="Animation"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public class AnimationSampler
    {
        /// <summary>
        /// Gets or Sets the animation that is being sampled.
        /// </summary>
        public Animation Animation { get; set; }

        /// <summary>
        /// Gets or Sets the 
        /// </summary>
        public SkeletonAnimationSampler? SkeletonAnimationSampler { get; set; }

        /// <summary>
        /// Gets the length of the animation
        /// </summary>
        public float Framerate { get; private set; }

        /// <summary>
        /// Gets the length of the animation.
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Gets the current time.
        /// </summary>
        public float CurrentTime { get; private set; }

        /// <summary>
        /// Gets or Sets the weights for this layer.
        /// </summary>
        public List<AnimationFrame<float>> Weights { get; set; }

        /// <summary>
        /// Gets or Sets the current weight
        /// </summary>
        public float CurrentWeight { get; set; }

        /// <summary>
        /// Gets or Sets the Weights Cursor
        /// </summary>
        private int CurrentWeightsCursor { get; set; }


        public AnimationSampler(Animation animation)
        {
            Animation = animation;
            Weights = new();

            if(Animation.SkeletonAnimation != null)
            {
                // If we didn't get passed a skeleton, we require one from
                // the animation.
                if(Animation.SkeletonAnimation.Skeleton == null)
                    throw new Exception();

                SkeletonAnimationSampler = new(
                    this,
                    Animation.SkeletonAnimation,
                    Animation.SkeletonAnimation.Skeleton);
            }
        }

        public AnimationSampler(Animation animation, Skeleton skeleton)
        {
            Animation = animation;
            Framerate = animation.Framerate;
            CurrentWeight = 1.0f;
            Weights = new();

            if (Animation.SkeletonAnimation != null)
            {
                SkeletonAnimationSampler = new(
                    this,
                    Animation.SkeletonAnimation,
                    skeleton);
            }
        }

        public void Update(float time) => Update(time, AnimationSampleType.AbsoluteFrameTime);

        public void Update(float time, AnimationSampleType type)
        {
            switch(type)
            {
                case AnimationSampleType.AbsoluteFrameTime:
                    CurrentTime = time;
                    break;
                case AnimationSampleType.AbsoluteTime:
                    CurrentTime = time * Framerate;
                    break;
                case AnimationSampleType.DeltaTime:
                    CurrentTime += time * Framerate;
                    break;
            }

            // Update our weight.
            var cursor = CurrentWeightsCursor;
            CurrentWeight = AnimationHelper.GetWeight(Weights, time, 1.0f, ref cursor);
            // Update our cursor to speed up linear sampling.
            CurrentWeightsCursor = cursor;

            // Update sub-samplers
            SkeletonAnimationSampler?.Update();
        }
    }
}
