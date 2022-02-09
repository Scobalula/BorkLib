using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// An enum that defines the data to allocate when initializing an instance of the <see cref="Mesh"/> class.
    /// </summary>
    [Flags]
    public enum MeshAttributeFlags : long
    {
        /// <summary>
        /// Defines if the mesh contains normals.
        /// </summary>
        Normals = 1 << 0,

        /// <summary>
        /// Defines if the mesh contains tangents.
        /// </summary>
        Tangents = 1 << 1,

        /// <summary>
        /// Defines if the mesh contains bitangents.
        /// </summary>
        BiTangents = 1 << 2,

        /// <summary>
        /// Defines if the mesh contains colours.
        /// </summary>
        Colours = 1 << 3,

        /// <summary>
        /// Defines if the mesh contains uv layers.
        /// </summary>
        UVLayers = 1 << 4,

        /// <summary>
        /// Defines if the mesh contains influences (bones).
        /// </summary>
        Influences = 1 << 5,

        /// <summary>
        /// Defines if the mesh contains basic normals and uvs.
        /// </summary>
        HasBasic = Normals | UVLayers,

        /// <summary>
        /// Defines if the mesh contains basic normals and uvs with skinning.
        /// </summary>
        BasicSkinned = Normals | UVLayers | Influences,

        /// <summary>
        /// Defines if the mesh contains extended data for normal mapping and colors.
        /// </summary>
        Extended = Normals | Tangents | BiTangents | Colours,

        /// <summary>
        /// Defines if the mesh contains extended data for normal mapping and colors with skinning.
        /// </summary>
        ExtendedSkinned = Normals | Tangents | BiTangents | Colours | UVLayers | Influences,

        /// <summary>
        /// Defines if the mesh contains all possible attributes.
        /// </summary>
        All = long.MaxValue,
    }
}
