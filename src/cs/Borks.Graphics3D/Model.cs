using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a 3D Model.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Gets or Sets the name of the model.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Get or Sets the skeleton this model uses.
        /// </summary>
        public Skeleton? Skeleton { get; set; }

        /// <summary>
        /// Get or Sets the morph this model uses.
        /// </summary>
        public Morph? Morph { get; set; }

        /// <summary>
        /// Gets or Sets the meshes stored within this model.
        /// </summary>
        public List<Mesh> Meshes { get; set; }

        /// <summary>
        /// Gets or Sets the materials stored within this model.
        /// </summary>
        public List<Material> Materials { get; set; }


        public Model()
        {
            Meshes = new();
            Materials = new();
        }

        public Model(Skeleton? skeleton)
        {
            Skeleton = skeleton;
            Meshes = new();
            Materials = new();
        }

        public Model(Skeleton? skeleton, Morph? morph)
        {
            Skeleton = skeleton;
            Morph = morph;
            Meshes = new();
            Materials = new();
        }

        /// <summary>
        /// Assigns the bone indices based off their index within the table. 
        /// </summary>
        public void AssignSkeletonBoneIndices()
        {
            Skeleton?.AssignBoneIndices();
        }
    }
}
