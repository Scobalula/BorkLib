using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a pointer to a texture file.
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// Gets or Sets the name of the texture.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the file path of the texture.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or Sets the texture type. For example diffuseMap, specularMap, etc.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class with the provided file.
        /// </summary>
        /// <param name="name">Texture name/file path.</param>
        /// <param name="type">Texture type. For example diffuseMap, specularMap, etc.</param>
        public Texture(string name, string type)
        {
            FilePath = name;
            Type = type;

            if ((Name = Path.GetFileNameWithoutExtension(name)) == null)
                Name = name;
        }
    }
}
