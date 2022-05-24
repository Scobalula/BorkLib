using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Borks.Graphics3D.CoDXAsset.Tokens;

namespace Borks.Graphics3D.CoDXAsset
{
    /// <summary>
    /// A class to help with reading CoD XModels.
    /// </summary>
    public static class CoDXModelHelper
    {
        /// <summary>
        /// Vertex Token Names
        /// </summary>
        internal static string[] VertexNames = new[] { "VERT", "VERT32"};

        /// <summary>
        /// Tri Token Names
        /// </summary>
        internal static string[] TriNames = new[] { "TRI", "TRI16" };

        public static void ReadBones(TokenReader reader, Skeleton skeleton)
        {
            var boneCount = reader.RequestNextTokenOfType<TokenDataUInt>("NUMBONES").Data;

            var boneInfos = new TokenDataBoneInfo[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                boneInfos[i] = reader.RequestNextTokenOfType<TokenDataBoneInfo>("BONE");

                if (boneInfos[i].BoneIndex != i)
                {
                    throw new InvalidDataException($"Bone index for: {boneInfos[i]} does not match current index.");
                }
            }

            for (int i = 0; i < boneCount; i++)
            {
                var boneIndex = reader.RequestNextTokenOfType<TokenDataUInt>("BONE").Data;
                var offset = reader.RequestNextTokenOfType<TokenDataVector3>("OFFSET").Data * 2.54f;
                var scale = Vector3.One;
                var nextToken = reader.RequestNextToken();

                if (nextToken?.Token.Name == "SCALE")
                {
                    if (nextToken is TokenDataVector3 scaleToken)
                    {
                        scale = scaleToken.Data;
                    }

                    nextToken = reader.RequestNextToken();
                }

                if (nextToken is not TokenDataVector3 xRowToken || nextToken.Token.Name != "X")
                    throw new Exception();


                var xRow = xRowToken.Data;
                var yRow = reader.RequestNextTokenOfType<TokenDataVector3>("Y").Data;
                var zRow = reader.RequestNextTokenOfType<TokenDataVector3>("Z").Data;
                var name = boneInfos[boneIndex].Name;
                var parent = boneInfos[boneIndex].BoneParentIndex;

                var matrix = new Matrix4x4(
                    xRow.X, xRow.Y, xRow.Z, 0,
                    yRow.X, yRow.Y, yRow.Z, 0,
                    zRow.X, zRow.Y, zRow.Z, 0,
                    0, 0, 0, 1
                    );

                skeleton.Bones.Add(new(name)
                {
                    Parent = /*parent > -1 ? skeleton.Bones[parent] : */null,
                    BaseWorldTranslation = offset,
                    BaseWorldRotation = Quaternion.CreateFromRotationMatrix(matrix),
                    BaseScale = scale
                });
            }

            skeleton.GenerateLocalTransforms();
        }

        public static void ReadGeometry(TokenReader reader, Model model, Skeleton skeleton)
        {
            var nextToken = reader.RequestNextToken();



            if (nextToken?.Token.Name != "NUMVERTS" && nextToken?.Token.Name != "NUMVERTS32")
                throw new IOException($"Expected NUMVERTS or NUMVERTS32 but got {nextToken?.Token.Name}");
            
            var vertCount    = (int)((TokenDataUInt)nextToken).Data;
            var boneCount    = skeleton.Bones.Count;
            var vertices     = new List<TokenDataVector3>(vertCount);
            var weightCounts = new List<(int, int)>(vertCount);
            var weights      = new List<TokenDataBoneWeight>(vertCount * 16);
            var instances    = new List<List<int>>(vertCount);

            nextToken = reader.RequestNextToken();

            for (int i = 0; i < vertCount; i++)
            {
                if (nextToken?.Token.Name != "VERT" && nextToken?.Token.Name != "VERT32")
                    throw new IOException($"Expected VERT or VERT32 but got {nextToken?.Token.Name}");

                vertices.Add(reader.RequestNextTokenOfType<TokenDataVector3>("OFFSET"));
                instances.Add(new());

                var weightCount = reader.RequestNextTokenOfType<TokenDataUInt>("BONES");

                weightCounts.Add((weights.Count, (int)weightCount.Data));

                for (int j = 0; j < weightCount.Data; j++)
                {
                    var weight = reader.RequestNextTokenOfType<TokenDataBoneWeight>("BONE");

                    if (weight.BoneIndex < 0 || weight.BoneIndex >= boneCount)
                        throw new IndexOutOfRangeException($"Bone index for vertice: {i} is out of range.");
                    if (weight.BoneWeight < 0)
                        throw new IndexOutOfRangeException($"Bone weight for vertice: {i} is negative.");

                    weights.Add(weight);
                }

                nextToken = reader.RequestNextToken();
            }

            if (nextToken?.Token.Name != "NUMFACES")
                throw new IOException($"Expected NUMFACES but got {nextToken?.Token.Name}");

            var faceCount = (int)((TokenDataUInt)nextToken).Data;

            var tris          = new List<TokenDataTri>(faceCount);
            var vertexIndices = new List<TokenDataUInt>(faceCount * 3);
            var normals       = new List<TokenDataVector3>(faceCount * 3);
            var colors        = new List<TokenDataVector4>(faceCount * 3);
            var uvLayers      = new List<TokenDataUVSet>(faceCount * 3);

            for (int i = 0; i < faceCount; i++)
            {
                tris.Add(reader.RequestNextTokenOfType<TokenDataTri>(TriNames));

                for (int v = 0; v < 3; v++)
                {
                    vertexIndices.Add(reader.RequestNextTokenOfType<TokenDataUInt>(VertexNames));
                    normals.Add(reader.RequestNextTokenOfType<TokenDataVector3>("NORMAL"));
                    colors.Add(reader.RequestNextTokenOfType<TokenDataVector4>("COLOR"));
                    uvLayers.Add(reader.RequestNextTokenOfType<TokenDataUVSet>("UV"));
                }
            }

            nextToken = reader.RequestNextToken();

            if (nextToken?.Token.Name != "NUMOBJECTS")
                throw new IOException($"Expected NUMOBJECTS but got {nextToken?.Token.Name}");

            var objCount = ((TokenDataUInt)nextToken).Data;

            for (int i = 0; i < objCount; i++)
            {
                // We don't care for this marvooning
                reader.RequestNextToken();
            }

            nextToken = reader.RequestNextToken();

            if (nextToken?.Token.Name != "NUMMATERIALS")
                throw new IOException($"Expected NUMMATERIALS but got {nextToken?.Token.Name}");

            var matCount = (int)((TokenDataUInt)nextToken).Data;
            var materials = new List<TokenDataUIntStringX3>(matCount);

            for (int i = 0; i < matCount; i++)
            {
                var material = reader.RequestNextTokenOfType<TokenDataUIntStringX3>("MATERIAL");

                if (material.IntegerValue != i)
                {
                    throw new InvalidDataException($"Material index for: {material.StringValue1} does not match current index.");
                }

                materials.Add(material);
                for (int s = 0; s < 12; s++)
                    reader.RequestNextToken();
            }

            // For each material, create a mesh, and a material entry.
            foreach (var material in materials)
            {
                var newMaterial = new Material(material.StringValue1);
                var newMesh = new Mesh();
                newMesh.Influences.SetCapacity(0, 8);
                newMesh.Materials.Add(newMaterial);

                model.Materials.Add(newMaterial);
                model.Meshes.Add(newMesh);
            }

            Span<int> indices = stackalloc int[3];

            // Build model.
            for (int i = 0, v = 0; i < faceCount; i++)
            {
                var tri = tris[i];
                var mesh = model.Meshes[tri.B];

                for (int j = 0; j < 3; j++, v++)
                {
                    var vertIndex = (int)vertexIndices[v].Data;

                    var position = vertices[vertIndex].Data * 2.54f;
                    var normal   = normals[v].Data;
                    var color    = colors[v].Data;
                    var uv       = uvLayers[v].UVs[0];
                    var existing = instances[vertIndex];
                    var faceIndex = -1;

                    // We need to check against other instances of faces
                    // using this vertex index, if we find an existing 
                    // instance of that vertex otherwise we end up with
                    // hundres of thousands of verts per face vertex
                    foreach (var c in existing)
                    {
                        if (mesh.Normals[c] == normal && mesh.UVLayers[c, 0] == uv)
                        {
                            faceIndex = c;
                            break;
                        }
                    }

                    if(faceIndex == -1)
                    {
                        faceIndex = mesh.Positions.Count;

                        mesh.Positions.Add(position);
                        mesh.Normals.Add(normal);
                        mesh.Colours.Add(color);
                        mesh.UVLayers.Add(uv);

                        var (weightsIdx, weightCount) = weightCounts[vertIndex];

                        for (int w = 0; w < weightCount; w++)
                        {
                            var weight = weights[weightsIdx + w];
                            mesh.Influences.Add((weight.BoneIndex, weight.BoneWeight), faceIndex, w);
                        }

                        existing.Add(faceIndex);
                    }

                    indices[j] = faceIndex;
                }

                // Match our format, CoD is reversed.
                mesh.Faces.Add((indices[0], indices[2], indices[1]));
            }
        }
    }
}
