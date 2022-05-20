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
        private AnimationSampler? MainLayer { get; set; }

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
            Skeleton = skeleton?.CreateCopy();
            MainLayer = new(anim, Skeleton);
            FrameRate = MainLayer.FrameRate;
            FrameCount = MainLayer.FrameCount;
        }

        public AnimationPlayer WithAnimation(string? name, Animation? anim)
        {
            if (name != null && anim != null)
            {
                if (Layers == null)
                    Layers = new();

                Layers[name] = new(anim, Skeleton);
            }
            return this;
        }

        public AnimationPlayer WithSolver(string? name, IAnimationSamplerSolver? solver)
        {
            if(solver != null && !string.IsNullOrWhiteSpace(name))
            {
                if (Solvers == null)
                    Solvers = new();

                Solvers = new();
            }

            return this;
        }

        public void Update(float time) => Update(
            time,
            AnimationSampleType.AbsoluteFrameTime);

        public void Update(float time, AnimationSampleType type)
        {
            if (MainLayer == null)
                throw new Exception(); // TODO: Exceptions

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
