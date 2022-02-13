using System.Drawing;
using System.Numerics;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a 3D Mesh with optional attributes.
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Gets or Sets the Positions
        /// </summary>
        public MeshAttributeCollection<Vector3> Positions { get; set; }

        /// <summary>
        /// Gets or Sets the Normals
        /// </summary>
        public MeshAttributeCollection<Vector3> Normals { get; set; }

        /// <summary>
        /// Gets or Sets the BiTangents
        /// </summary>
        public MeshAttributeCollection<Vector3> BiTangents { get; set; }

        /// <summary>
        /// Gets or Sets the Tangents
        /// </summary>
        public MeshAttributeCollection<Vector3> Tangents { get; set; }

        /// <summary>
        /// Gets or Sets the Colours
        /// </summary>
        public MeshAttributeCollection<Vector4> Colours { get; set; }

        /// <summary>
        /// Gets or Sets the UV Layers
        /// </summary>
        public MeshAttributeCollection<Vector2> UVLayers { get; set; }

        /// <summary>
        /// Gets or Sets the influences for use in skeletal animation. The index points to the skeleton bone in the main model that owns this mesh.
        /// </summary>
        public MeshAttributeCollection<(int, float)> Influences { get; set; }

        /// <summary>
        /// Gets or Sets the delta positions for use in morph animations. The index points to the morph target in the main model that owns this mesh.
        /// </summary>
        public MeshAttributeCollection<(int, Vector3)> DeltaPositions { get; set; }

        /// <summary>
        /// Gets or Sets the delta normals for use in morph animations. The index points to the morph target in the main model that owns this mesh.
        /// </summary>
        public MeshAttributeCollection<(int, Vector3)> DeltaNormals { get; set; }

        /// <summary>
        /// Gets or Sets the delta tangents for use in morph animations. The index points to the morph target in the main model that owns this mesh.
        /// </summary>
        public MeshAttributeCollection<(int, Vector3)> DeltaTangents { get; set; }

        /// <summary>
        /// Gets or Sets the materials assigned to this mesh.
        /// </summary>
        public List<Material> Materials { get; set; }

        /// <summary>
        /// Gets or Sets the Polygon Face Indices
        /// </summary>
        public MeshAttributeCollection<(int, int, int)> Faces { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh()
        {
            Positions      = new();
            Faces          = new();
            Normals        = new();
            Tangents       = new();
            BiTangents     = new();
            Colours        = new();
            UVLayers       = new();
            Influences     = new();
            Materials      = new();
            DeltaPositions = new();
            DeltaNormals   = new();
            DeltaTangents  = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh(Mesh from)
        {
            Positions      = new(from.Positions);
            Faces          = new(from.Faces);
            Normals        = new(from.Normals);
            Tangents       = new(from.Tangents);
            BiTangents     = new(from.BiTangents);
            Colours        = new(from.Colours);
            UVLayers       = new(from.UVLayers);
            Influences     = new(from.Influences);
            Materials      = new(from.Materials);
            DeltaPositions = new(from.DeltaPositions);
            DeltaNormals   = new(from.DeltaNormals);
            DeltaTangents  = new(from.DeltaTangents);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class with the provided parameters.
        /// </summary>
        /// <param name="vertexCount">Number of vertices this mesh contains.</param>
        /// <param name="faceCount">Number of faces this mesh contains.</param>
        /// <param name="uvLayers">Number of UV Layers, if none, set to 0.</param>
        /// <param name="influences">Number of influences, if none, set to 0.</param>
        /// <param name="morphs">Number of morph targets, if none, set to 0.</param>
        /// <param name="flags">The <see cref="MeshAttributeFlags>"/> that define what data to allocate.</param>
        public Mesh(int vertexCount, int faceCount, int uvLayers, int influences, int morphs, MeshAttributeFlags flags)
        {
            // At the very least we need positions and faces
            // for a polygon mesh.
            Positions      = new(vertexCount);
            Faces          = new(faceCount);
            Positions      = new();
            Faces          = new();
            Normals        = new();
            Tangents       = new();
            BiTangents     = new();
            Colours        = new();
            UVLayers       = new();
            Influences     = new();
            Materials      = new();
            DeltaPositions = new();
            DeltaNormals   = new();
            DeltaTangents  = new();

            if (flags.HasFlag(MeshAttributeFlags.Normals))
                Normals.SetCapacity(vertexCount);
            if (flags.HasFlag(MeshAttributeFlags.Tangents))
                Tangents.SetCapacity(vertexCount);
            if (flags.HasFlag(MeshAttributeFlags.BiTangents))
                BiTangents.SetCapacity(vertexCount);
            if (flags.HasFlag(MeshAttributeFlags.Colours))
                Colours.SetCapacity(vertexCount);
            if (flags.HasFlag(MeshAttributeFlags.UVLayers))
                UVLayers.SetCapacity(vertexCount, uvLayers);
            if (flags.HasFlag(MeshAttributeFlags.Influences))
                Influences.SetCapacity(vertexCount, influences);
            if (flags.HasFlag(MeshAttributeFlags.DeltaPositions))
                DeltaPositions.SetCapacity(vertexCount, morphs);
            if (flags.HasFlag(MeshAttributeFlags.DeltaNormals))
                DeltaNormals.SetCapacity(vertexCount, morphs);
            if (flags.HasFlag(MeshAttributeFlags.DeltaTangents))
                DeltaTangents.SetCapacity(vertexCount, morphs);
        }
    }
}