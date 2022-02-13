using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold morphs for <see cref="Model"/>s and <see cref="Mesh"/>s.
    /// </summary>
    public class Morph
    {
        /// <summary>
        /// Gets or Sets the morph targets.
        /// </summary>
        public List<MorphTarget> Targets { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Morph"/> class.
        /// </summary>
        public Morph()
        {
            Targets = new();
        }
    }
}
