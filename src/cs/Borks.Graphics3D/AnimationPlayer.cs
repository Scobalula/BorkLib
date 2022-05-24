using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Borks.Graphics3D.AnimationSampling;

namespace Borks.Graphics3D
{
    public class AnimationPlayer
    {
        /// <summary>
        /// Gets the skeleton.
        /// </summary>
        public Skeleton? Skeleton { get; private set; }

        /// <summary>
        /// Gets or Sets the main layer.
        /// </summary>
        public AnimationSampler MainLayer { get; set; }

        /// <summary>
        /// Gets or Sets the layers.
        /// </summary>
        private Dictionary<string, AnimationSampler>? Layers { get; set; }

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public float FrameCount { get; set; }

        /// <summary>
        /// Gets or Sets the playback framerate.
        /// </summary>
        public float FrameRate { get; set; }

        /// <summary>
        /// Gets the length of the animation.
        /// </summary>
        public float Length => FrameCount / FrameRate;

        /// <summary>
        /// Gets the length of each frame.
        /// </summary>
        public float FrameTime => 1.0f / FrameRate;

        /// <summary>
        /// Gets or Sets the solvers.
        /// </summary>
        private Dictionary<string, IAnimationSamplerSolver>? Solvers { get; set; }

        public AnimationPlayer(Animation anim, Skeleton? skeleton)
        {
            Skeleton = skeleton;
            MainLayer = new(anim, Skeleton);
            FrameRate = MainLayer.FrameRate;
            FrameCount = MainLayer.FrameCount;

            // Ensure skeleton is cleared
            Skeleton?.InitializeAnimationTransforms();
        }

        public AnimationPlayer WithSubLayer(string? name, Animation? anim)
        {
            if (name != null && anim != null)
            {
                if (Layers == null)
                    Layers = new();

                var nLayer = new AnimationSampler(anim, Skeleton);
                FrameCount = Math.Max(nLayer.FrameCount, FrameCount);
                Layers[name] = nLayer;
            }
            return this;
        }

        public AnimationPlayer WithSubLayer(string? name, Animation? anim, float startTime)
        {
            if (name != null && anim != null)
            {
                if (Layers == null)
                    Layers = new();

                var nLayer = new AnimationSampler(anim, Skeleton)
                {
                    StartFrame = startTime
                };

                FrameCount = Math.Max(nLayer.FrameCount + nLayer.StartFrame, FrameCount);
                Layers[name] = nLayer;
            }
            return this;
        }

        public AnimationPlayer WithSubLayer(string? name, Animation? anim, float startTime, float blend)
        {
            if (name != null && anim != null)
            {
                if (Layers == null)
                    Layers = new();

                var nLayer = new AnimationSampler(anim, Skeleton);

                nLayer.Weights.Add(new(0, 0));
                nLayer.Weights.Add(new(nLayer.FrameCount * blend, 1));
                nLayer.StartFrame = startTime;

                FrameCount = Math.Max(nLayer.FrameCount + nLayer.StartFrame, FrameCount);
                Layers[name] = nLayer;
            }
            return this;
        }

        public AnimationPlayer WithSolver(string? name, IAnimationSamplerSolver? solver)
        {
            if(solver != null && !string.IsNullOrWhiteSpace(name))
            {
                if (Solvers == null)
                    Solvers = new();

                Solvers[name] = solver;
            }

            return this;
        }

        public void Update(float time) => Update(
            time,
            AnimationSampleType.AbsoluteFrameTime);

        public void Update(float time, AnimationSampleType type)
        {
            // TODO: Could do with some refactoring to not
            // make this necessary.
            Skeleton?.InitializeAnimationTransforms();
            MainLayer.Update(time, type);

            if(Layers != null)
            {
                foreach (var layer in Layers)
                {
                    layer.Value.Update(time, type);
                }
            }

            if(Solvers != null)
            {
                foreach (var solver in Solvers)
                {
                    solver.Value.Update(MainLayer.CurrentTime);
                }
            }
        }

        public AnimationSampler? GetLayer(string name) => Layers != null ? Layers.TryGetValue(name, out var sampler) ? sampler : null : null;

        public bool TryGetLayer(string name,[NotNullWhen(true)] out AnimationSampler? sampler) => (sampler = GetLayer(name)) != null;

        public IAnimationSamplerSolver? GetSolver(string name) => Solvers != null ? Solvers.TryGetValue(name, out var solver) ? solver : null : null;

        public bool TryGetSolver(string name, [NotNullWhen(true)] out IAnimationSamplerSolver? solver) => (solver = GetSolver(name)) != null;
    }
}
