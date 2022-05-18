using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.SMD
{
    /// <summary>
    /// Provides helper methods for working with SMD files.
    /// </summary>
    internal static class SMDHelper
    {
        /// <summary>
        /// Converts the provided <see cref="Quaternion"/> to euler angles that SMD expects.
        /// </summary>
        public static Vector3 QuaternionToEulerAngles(Quaternion quaternion)
        {
            Vector3 result = new();

            float t0 = 2.0f * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            float t1 = 1.0f - 2.0f * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);

            result.X = MathF.Atan2(t0, t1);


            float t2 = 2.0f * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);

            t2 = t2 > 1.0f ? 1.0f : t2;
            t2 = t2 < -1.0f ? -1.0f : t2;
            result.Y = MathF.Asin(t2);


            float t3 = +2.0f * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            float t4 = +1.0f - 2.0f * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);

            result.Z = MathF.Atan2(t3, t4);

            return result;
        }

        /// <summary>
        /// Reads the nodes from the provided <see cref="SMDReader"/>.
        /// </summary>
        public static void ReadNodes(SMDReader reader, Skeleton skeleton)
        {
            var bones = new List<SkeletonBone>();
            var boneIDs = new List<int>();
            var boneParents = new List<int>();

            while (true)
            {
                var token = reader.Parse();

                if (token.SequenceEqual("end"))
                    break;
                if (token.Length == 0)
                    break;

                // According to Valve Wiki
                // this isn't sequential, but all
                // files I've ran into are, but with this 
                // in mind, we will do this
                var id = int.Parse(token);
                var name = reader.Parse();
                var parentID = int.Parse(reader.Parse());

                bones.Add(new(new(name)));
                boneIDs.Add(id);
                boneParents.Add(parentID);
                skeleton.Bones.Add(null!);
            }

            // Now assign based off ID
            for (int i = 0; i < boneIDs.Count; i++)
            {
                skeleton.Bones[boneIDs[i]] = bones[i];
            }

            for (int i = 0; i < boneParents.Count; i++)
            {
                if(boneParents[i] != -1)
                {
                    skeleton.Bones[i].Parent = skeleton.Bones[boneParents[i]];
                }
            }
        }

        /// <summary>
        /// Reads the triangles from the provided <see cref="SMDReader"/>.
        /// </summary>
        public static void ReadSkeleton(SMDReader reader, Skeleton skeleton, out Animation? animation)
        {
            animation = null;
            int currentTime = 0;

            while (true)
            {
                var token = reader.Parse();

                if (token.SequenceEqual("end"))
                    break;
                if (token.Length == 0)
                    break;

                // New time
                if(token.SequenceEqual("time"))
                {
                    currentTime = int.Parse(reader.Parse());
                    continue;
                }

                // If we are above time 0, ensure our animation is created
                if (currentTime > 0 && animation == null)
                {
                    // Assume a relative animation, SMD has no indicator of type.
                    animation = new(skeleton);
                    animation.SkeletonAnimation!.TransformType = TransformType.Relative;

                    foreach (var bone in skeleton.Bones)
                    {
                        var target = new SkeletonAnimationTarget(bone.Name)
                        {
                            // Set the first frame to our bone's base
                            TranslationFrames = new()
                            {
                                new(0, bone.BaseLocalTranslation)
                            },
                            RotationFrames = new()
                            {
                                new(0, bone.BaseLocalRotation)
                            }
                        };

                        animation.SkeletonAnimation!.Targets.Add(target);

                    }
                }

                var boneID = int.Parse(token);
                var posX   = float.Parse(reader.Parse());
                var posY   = float.Parse(reader.Parse());
                var posZ   = float.Parse(reader.Parse());
                var rotX   = float.Parse(reader.Parse());
                var rotY   = float.Parse(reader.Parse());
                var rotZ   = float.Parse(reader.Parse());

                // Convert to matrix then to quat
                // TODO: Use direct to Quat
                var toMatrix = Matrix4x4.CreateRotationX(rotX) *
                               Matrix4x4.CreateRotationY(rotY) *
                               Matrix4x4.CreateRotationZ(rotZ);
                var toQuat = Quaternion.CreateFromRotationMatrix(toMatrix);
                var position = new Vector3(posX, posY, posZ);

                // Either add to our animation, or add to our skeleton if we're time 0.
                if (currentTime > 0)
                {
                    animation!.SkeletonAnimation!.Targets[boneID].TranslationFrames!.Add(new(currentTime, position));
                    animation!.SkeletonAnimation!.Targets[boneID].RotationFrames!.Add(new(currentTime, toQuat));
                }
                else
                {

                    skeleton.Bones[boneID].BaseLocalTranslation = position;
                    skeleton.Bones[boneID].BaseLocalRotation = toQuat;
                }
            }
        }

        /// <summary>
        /// Reads the a vertex from the provided <see cref="SMDReader"/>.
        /// </summary>
        public static void ReadVertex(SMDReader reader, Mesh mesh)
        {
            var idx = mesh.Positions.Count;

            var parentBone = int.Parse(reader.Parse());
            var posX       = float.Parse(reader.Parse(false));
            var posY       = float.Parse(reader.Parse(false));
            var posZ       = float.Parse(reader.Parse(false));
            var norX       = float.Parse(reader.Parse(false));
            var norY       = float.Parse(reader.Parse(false));
            var norZ       = float.Parse(reader.Parse(false));
            var uvX        = float.Parse(reader.Parse(false));
            var uvY        = float.Parse(reader.Parse(false));

            mesh.Positions.Add(new(posX, posY, posZ));
            mesh.Normals.Add(Vector3.Normalize(new(norX, norY, norZ)));
            mesh.UVLayers.Add(new(uvX, uvY));

            var boneCountToken = reader.Parse(false);

            // Check for weight maps
            if(boneCountToken.Length > 0)
            {
                var boneCount = int.Parse(boneCountToken);

                for (int i = 0; i < boneCount; i++)
                {
                    var boneIndex = int.Parse(reader.Parse(false));
                    var boneWeight = float.Parse(reader.Parse(false));

                    mesh.Influences.Add(new(boneIndex, boneWeight), idx, i);
                }
            }
            else
            {
                mesh.Influences.Add(new(parentBone, 1.0f));
            }

            reader.SkipToNextLine();
        }

        /// <summary>
        /// Reads the triangles from the provided <see cref="SMDReader"/>.
        /// </summary>
        public static void ReadTriangles(SMDReader reader, Skeleton skeleton, out Model? model)
        {
            var meshTable = new Dictionary<string, Mesh>();

            while (true)
            {
                var token = reader.Parse();

                if (token.SequenceEqual("end"))
                    break;
                if (token.Length == 0)
                    break;

                var materialName = new string(token);

                if(!meshTable.TryGetValue(materialName, out var mesh))
                {
                    mesh = new();
                    meshTable[materialName] = mesh;
                }

                // Store current number of positions
                // as we'll use this for face indices
                int index = mesh.Positions.Count;

                ReadVertex(reader, mesh);
                ReadVertex(reader, mesh);
                ReadVertex(reader, mesh);

                // Add our new unique positions
                // any merging can be done if the user wants
                // later
                mesh.Faces.Add(new(index, index + 1, index + 2));
            }

            model = new(skeleton);

            foreach (var (materialName, mesh) in meshTable)
            {
                var material = new Material(materialName);

                mesh.Materials.Add(material);
                model.Materials.Add(material);
                model.Meshes.Add(mesh);
            }
        }

        /// <summary>
        /// Reads the triangles from the provided <see cref="SMDReader"/>.
        /// <summary>
        public static void Read(SMDReader reader, out Skeleton? skeleton, out Model? model, out Animation? animation)
        {
            model = null;
            animation = null;
            skeleton = new Skeleton();

            var versionToken = reader.Parse();

            if (!versionToken.SequenceEqual("version"))
                throw new SMDReaderException("Expecting 'version' token at start of file.", reader.Line, reader.Column);

            var verion = reader.Parse();

            while(true)
            {
                var token = reader.Parse();

                if(token.Length == 0)
                {
                    break;
                }

                if(token.SequenceEqual("nodes"))
                {
                    ReadNodes(reader, skeleton);
                }
                else if(token.SequenceEqual("skeleton"))
                {
                    ReadSkeleton(reader, skeleton, out animation);
                }
                else if(token.SequenceEqual("triangles"))
                {
                    ReadTriangles(reader, skeleton, out model);
                }
            }
        }

        /// <summary>
        /// Writes an SMD Frame to the writer.
        /// </summary>
        public static void WriteFrame(TextWriter writer, int boneIndex, Vector3 position, Quaternion rotation)
        {
            var euler = QuaternionToEulerAngles(rotation);

            writer.Write($"{boneIndex} ");
            writer.Write($"{position.X} {position.Y} {position.Z} ");
            writer.Write($"{euler.X} {euler.Y} {euler.Z} ");

            writer.WriteLine();
        }

        /// <summary>
        /// Writes an SMD vertex for a triangle.
        /// </summary>
        public static void WriteVertex(TextWriter writer, int vertexIndex, Mesh mesh, bool hasNormals, bool hasUVs, bool hasWeights)
        {
            var pos    = mesh.Positions[vertexIndex];
            var normal = hasNormals ? Vector3.Normalize(mesh.Normals[vertexIndex]) : Vector3.UnitX;
            var uv     = hasUVs ? mesh.UVLayers[vertexIndex, 0] : Vector2.Zero;

            writer.Write($"0 ");
            writer.Write($"{pos.X} {pos.Y} {pos.Z} ");
            writer.Write($"{normal.X} {normal.Y} {normal.Z} ");
            writer.Write($"{uv.X} {uv.Y} ");

            if(hasWeights)
            {
                var weightCount = mesh.Influences.FindIndex(vertexIndex, x => x.Item2 == 0.0f);

                if (weightCount == -1)
                    weightCount = mesh.Influences.Dimension;

                writer.Write($"{weightCount} ");

                for (int i = 0; i < weightCount; i++)
                {
                    var (index, value) = mesh.Influences[vertexIndex, i];

                    writer.Write($"{index} {value} ");
                }
            }

            writer.WriteLine();
        }
    }
}
