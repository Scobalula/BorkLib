using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold the result of what is read by an <see cref="Graphics3DTranslator"/> instance.
    /// </summary>
    public class Graphics3DTranslatorIO
    {
        /// <summary>
        /// Gets or Sets any skeletons read by the translator.
        /// </summary>
        public List<Skeleton> Skeletons { get; set; }

        /// <summary>
        /// Gets or Sets any models read by the translator.
        /// </summary>
        public List<Model> Models { get; set; }

        /// <summary>
        /// Gets or Sets any animations read by the translator.
        /// </summary>
        public List<Animation> Animations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics3DTranslatorIO"/> class.
        /// </summary>
        public Graphics3DTranslatorIO()
        {
            Models = new();
            Animations = new();
            Skeletons = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics3DTranslatorIO"/> class.
        /// </summary>
        public Graphics3DTranslatorIO(Model model)
        {
            Models = new();
            Animations = new();
            Skeletons = new();

            Models.Add(model);
        }

        /// <summary>
        /// Attempts to get the first skeleton, if none provided, tries to aquire it from the animation or models provided.
        /// </summary>
        /// <param name="skeleton">The resulting skeleton.</param>
        /// <returns>True if found, otherwise false.</returns>
        public bool TryGetFirstSkeleton([NotNullWhen(true)] out Skeleton? skeleton)
        {
            skeleton = null;

            // Check for the se
            if(Skeletons.Count > 0)
            {
                skeleton = Skeletons[0];
                return true;
            }
            // Check for it in models
            if(Models.Count > 0 && Models[0].Skeleton != null)
            {
                skeleton = Models[0].Skeleton!;
                return true;
            }
            // Check for it in animations
            if (Animations.Count > 0 && Animations[0].SkeletonAnimation?.Skeleton != null)
            {
                skeleton = Animations[0].SkeletonAnimation!.Skeleton!;
                return true;
            }

            return false;
        }
    }
}
