using System;
using System.Collections.Generic;
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics3DTranslatorIO"/> class.
        /// </summary>
        public Graphics3DTranslatorIO(Model model)
        {
            Models = new();
            Animations = new();
            Models.Add(model);
        }
    }
}
