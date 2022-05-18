using System.Diagnostics.CodeAnalysis;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold the result of what is read by an <see cref="Graphics3DTranslator"/> instance.
    /// </summary>
    public class Graphics3DTranslatorIO
    {
        /// <summary>
        /// Gets or Sets any objects read by the translator.
        /// </summary>
        public List<Graphics3DObject> Objects { get; set; }

        /// <summary>
        /// Gets or Sets the scale to apply on export if supported by the translator.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics3DTranslatorIO"/> class.
        /// </summary>
        public Graphics3DTranslatorIO()
        {
            Objects = new();
            Scale = 1.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics3DTranslatorIO"/> class.
        /// </summary>
        public Graphics3DTranslatorIO(Graphics3DObject obj)
        {
            Objects = new();
            Scale = 1.0f;

            Objects.Add(obj);
        }

        public T? GetFirstInstance<T>() where T : Graphics3DObject
        {
            var type = typeof(T);
            return (T?)Objects.FirstOrDefault(x => x.GetType() == type);
        }

        /// <summary>
        /// Attempts to get the first skeleton, if none provided, tries to aquire it from the animation or models provided.
        /// </summary>
        /// <param name="skeleton">The resulting skeleton.</param>
        /// <returns>True if found, otherwise false.</returns>
        public bool TryGetFirstSkeleton([NotNullWhen(true)] out Skeleton? skeleton)
        {
            skeleton = GetFirstInstance<Skeleton>();
            if(skeleton == null)
                skeleton = GetFirstInstance<Model>()?.Skeleton;
            if (skeleton == null)
                skeleton = GetFirstInstance<Animation>()?.SkeletonAnimation?.Skeleton;
            return skeleton != null;
        }
    }
}
