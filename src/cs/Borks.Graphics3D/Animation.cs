using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a 3D Animation that animations skeletons and/or other data.
    /// </summary>
    public class Animation : Graphics3DObject
    {
        /// <summary>
        /// Gets or Sets the skeleton animation stored within this animation.
        /// </summary>
        public SkeletonAnimation? SkeletonAnimation { get; set; }

        /// <summary>
        /// Gets or Sets the animation actions.
        /// </summary>
        public List<AnimationAction> Actions { get; set; }

        /// <summary>
        /// Gets or Sets the animation frame rate.
        /// </summary>
        public float Framerate { get; set; }

        /// <summary>
        /// Gets the number of skeletal animation targets.
        /// </summary>
        public int SkeletalTargetCount => SkeletonAnimation != null ? SkeletonAnimation.Targets.Count : 0;

        public bool TryGetAction(string v,[NotNullWhen(true)] out AnimationAction? action)
        {
            action = Actions.Find(x => x.Name.Equals(v));
            return action != null;
        }

        /// <summary>
        /// Gets the skeletal animation transform type.
        /// </summary>
        public TransformType SkeletalTransformType => SkeletonAnimation != null ? SkeletonAnimation.TransformType : TransformType.Unknown;


        /// <summary>
        /// Initializes a new instance of the <see cref="Animation"/> class.
        /// </summary>
        public Animation()
        {
            Framerate = 30.0f;
            Actions = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Animation"/> class.
        /// </summary>
        /// <param name="skeleton">Skeleton to assign to the instance of the <see cref="SkeletonAnimation"/>.</param>
        public Animation(Skeleton skeleton)
        {
            Framerate = 30.0f;
            SkeletonAnimation = new(skeleton);
            Actions = new();
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

                foreach (var action in Actions)
                {
                    foreach (var f in action.Frames)
                    {
                        minFrame = MathF.Min(minFrame, f.Time);
                        maxFrame = MathF.Max(maxFrame, f.Time);
                    }
                }
            }

            return (maxFrame - minFrame) + 1;
        }

        /// <summary>
        /// Calculates the total number of actions in this animation.
        /// </summary>
        /// <returns>The total number of actions in this animation.</returns>
        public int GetAnimationActionCount()
        {
            var actionCount = 0;

            foreach (var action in Actions)
            {
                actionCount += action.Frames.Count;
            }

            return actionCount;
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
                    if(target.TranslationFrames != null && target.TranslationFrames.Count >= 0)
                    {
                        return true;
                    }
                }
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
                    if (target.RotationFrames != null && target.RotationFrames.Count >= 0)
                    {
                        return true;
                    }
                }
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
                    if (target.ScaleFrames != null && target.ScaleFrames.Count >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new instance of an <see cref="AnimationAction"/> within this animation, if one already exists with this name, then that action is returned.
        /// </summary>
        /// <param name="name">Name of the action.</param>
        /// <returns>A new action that is added to this animation if it doesn't exist, otherwise an existing action with the given name.</returns>
        public AnimationAction CreateAction(string name)
        {
            var idx = Actions.FindIndex(x => x.Name == name);

            if (idx != -1)
                return Actions[idx];

            var nAction = new AnimationAction(name, "Default");
            Actions.Add(nAction);

            return nAction;
        }
    }
}
