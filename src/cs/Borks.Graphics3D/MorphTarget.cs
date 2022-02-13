using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a morph target.
    /// </summary>
    public class MorphTarget
    {
        /// <summary>
        /// Gets or Sets the name of the morph target.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MorphTarget"/> class.
        /// </summary>
        /// <param name="name">The name of the morph target.</param>
        public MorphTarget(string name)
        {
            Name = name;
        }
    }
}
