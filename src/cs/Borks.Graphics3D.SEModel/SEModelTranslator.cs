using System.Numerics;
using System.Text;

namespace Borks.Graphics3D.SEModel
{
    /// <summary>
    /// A class to translate a Model to SEModel
    /// </summary>
    public sealed class SEModelTranslator : Graphics3DTranslator
    {
        /// <summary>
        /// SEModel Magic
        /// </summary>
        public static readonly byte[] Magic = { 0x53, 0x45, 0x4D, 0x6F, 0x64, 0x65, 0x6C };

        /// <inheritdoc/>
        public override string Name => "SEModelTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        {
            ".semodel"
        };

        /// <inheritdoc/>
        public override bool SupportsReading => true;

        /// <inheritdoc/>
        public override bool SupportsWriting => true;

        /// <inheritdoc/>
        public override void Read(Stream stream, Graphics3DTranslatorIO output)
        {
            // SEModels can only contain a single skeleton
            // and model.
            var scale = 1.0f;
            var skeleton = new Skeleton();
            var result = new Model(skeleton);

            output.Models.Add(result);

            using var reader = new BinaryReader(stream, Encoding.Default, true);

            if (!Magic.SequenceEqual(reader.ReadBytes(Magic.Length)))
                throw new IOException("Invalid SEModel Magic");
            if (reader.ReadUInt16() != 0x1)
                throw new IOException("Invalid SEModel Version");
            if (reader.ReadUInt16() != 0x14)
                throw new IOException("Invalid SEModel Header Size");

            var dataPresence = reader.ReadByte();
            var boneDataPresence = reader.ReadByte();
            var meshDataPresence = reader.ReadByte();

            var boneCount = reader.ReadInt32();
            var meshCount = reader.ReadInt32();
            var matCount = reader.ReadInt32();

            var reserved0 = reader.ReadByte();
            var reserved1 = reader.ReadByte();
            var reserved2 = reader.ReadByte();

            var boneNames = new string[boneCount];
            var boneParents = new int[boneCount];

            for (int i = 0; i < boneNames.Length; i++)
            {
                boneNames[i] = ReadUTF8String(reader);
            }

            var hasWorldTransforms = (boneDataPresence & (1 << 0)) != 0;
            var hasLocalTransforms = (boneDataPresence & (1 << 1)) != 0;
            var hasScaleTransforms = (boneDataPresence & (1 << 2)) != 0;

            for (int i = 0; i < boneCount; i++)
            {
                // For now, this flag is unused, and so a non-zero indicates
                // something is wrong in our book
                if (reader.ReadByte() != 0)
                    throw new IOException("Invalid SEModel Bone Flag");

                boneParents[i] = reader.ReadInt32();

                var bone = new SkeletonBone(boneNames[i]);

                if (hasWorldTransforms)
                {
                    bone.LocalPosition = new Vector3(
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale);
                    bone.GlobalRotation = new Quaternion(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
                }
                else
                {
                    bone.GlobalPosition = Vector3.Zero;
                    bone.GlobalRotation = Quaternion.Identity;
                }

                if (hasLocalTransforms)
                {
                    bone.LocalPosition = new Vector3(
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale,
                        reader.ReadSingle()) * scale;
                    bone.LocalRotation = new Quaternion(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
                }
                else
                {
                    bone.LocalPosition = Vector3.Zero;
                    bone.LocalRotation = Quaternion.Identity;
                }

                if (hasScaleTransforms)
                {
                    bone.Scale = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
                }

                result.Skeleton!.Bones.Add(bone);
            }

            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                if (boneParents[i] != -1)
                {
                    skeleton.Bones[i].Parent = skeleton.Bones[boneParents[i]];
                }
            }

            var hasUVs = (meshDataPresence & (1 << 0)) != 0;
            var hasNormals = (meshDataPresence & (1 << 1)) != 0;
            var hasColours = (meshDataPresence & (1 << 2)) != 0;
            var hasWeights = (meshDataPresence & (1 << 3)) != 0;

            var materialIndices = new List<int>[meshCount];

            result.Meshes = new List<Mesh>();

            for (int i = 0; i < meshCount; i++)
            {
                // For now, this flag is unused, and so a non-zero indicates
                // something is wrong in our book
                if (reader.ReadByte() != 0)
                    throw new IOException("Invalid SEModel Mesh Flag");

                var layerCount = reader.ReadByte();
                var influences = reader.ReadByte();
                var vertexCount = reader.ReadInt32();
                var faceCount = reader.ReadInt32();

                var mesh = new Mesh();

                // Not necessary but initializes the collection capacity 
                // so we're not reallocating
                mesh.Positions.SetCapacity(vertexCount);
                mesh.Faces.SetCapacity(faceCount);


                // Positions
                for (int v = 0; v < vertexCount; v++)
                {
                    mesh.Positions.Add(new(
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale));
                }
                // UVs
                if (hasUVs)
                {
                    mesh.UVLayers.SetCapacity(vertexCount, layerCount);

                    for (int v = 0; v < vertexCount; v++)
                    {
                        for (int l = 0; l < layerCount; l++)
                        {
                            mesh.UVLayers.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()), v, l);
                        }
                    }
                }
                // Normals
                if (hasNormals)
                {
                    mesh.Normals.SetCapacity(vertexCount);

                    for (int v = 0; v < vertexCount; v++)
                    {
                        mesh.Normals.Add(new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                    }
                }
                // Colours
                if (hasColours)
                {
                    mesh.Colours.SetCapacity(vertexCount);

                    for (int v = 0; v < vertexCount; v++)
                    {
                        mesh.Colours.Add(new(
                            reader.ReadByte() / 255.0f,
                            reader.ReadByte() / 255.0f,
                            reader.ReadByte() / 255.0f,
                            reader.ReadByte() / 255.0f));
                    }
                }
                // Weights
                if (hasWeights)
                {
                    mesh.Influences = new(vertexCount, influences);

                    for (int v = 0; v < vertexCount; v++)
                    {
                        for (int w = 0; w < influences; w++)
                        {
                            mesh.Influences.Add(new(
                                boneCount <= 0xFF ? reader.ReadByte() :
                                boneCount <= 0xFFFF ? reader.ReadUInt16() :
                                reader.ReadInt32(),
                                reader.ReadSingle()), v, w);
                        }
                    }
                }
                // Faces
                for (int f = 0; f < faceCount; f++)
                {
                    if (vertexCount <= 0xFF)
                    {
                        mesh.Faces.Add(new(
                            reader.ReadByte(),
                            reader.ReadByte(),
                            reader.ReadByte()));
                    }
                    else if (vertexCount <= 0xFFFF)
                    {
                        mesh.Faces.Add(new(
                            reader.ReadUInt16(),
                            reader.ReadUInt16(),
                            reader.ReadUInt16()));
                    }
                    else
                    {
                        mesh.Faces.Add(new(
                            reader.ReadInt32(),
                            reader.ReadInt32(),
                            reader.ReadInt32()));
                    }
                }

                materialIndices[i] = new List<int>(layerCount);

                for (int m = 0; m < layerCount; m++)
                {
                    materialIndices[i].Add(reader.ReadInt32());
                }

                result.Meshes.Add(mesh);
            }

            for (int i = 0; i < matCount; i++)
            {
                var mtl = new Material(ReadUTF8String(reader));

                if (reader.ReadBoolean())
                {
                    mtl.Textures["DiffuseMap"] = new(ReadUTF8String(reader), "DiffuseMap");
                    mtl.Textures["NormalMap"] = new(ReadUTF8String(reader), "DiffuseMap");
                    mtl.Textures["SpecularMap"] = new(ReadUTF8String(reader), "DiffuseMap");
                }

                result.Materials.Add(mtl);
            }

            // Last pass for materials
            for (int i = 0; i < result.Meshes.Count; i++)
            {
                foreach (var index in materialIndices[i])
                    result.Meshes[i].Materials.Add(result.Materials[index]);
            }
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, Graphics3DTranslatorIO input)
        {
            var data = input.Models.First();
            var scale = 1.0f;

            using var writer = new BinaryWriter(stream, Encoding.Default, true);

            writer.Write(Magic);
            writer.Write((ushort)0x1);
            writer.Write((ushort)0x14);
            writer.Write((byte)0x7); // Data Presence
            writer.Write((byte)0x7); // Bone Data Presence
            writer.Write((byte)0xF); // Mesh Data Presence
            writer.Write(data.Skeleton.Bones.Count);
            writer.Write(data.Meshes.Count);
            writer.Write(data.Materials.Count);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);

            var indexTable = new int[data.Skeleton.Bones.Count];

            for (int i = 0; i < data.Skeleton.Bones.Count; i++)
            {
                writer.Write(Encoding.ASCII.GetBytes(data.Skeleton.Bones[i].Name));
                writer.Write((byte)0);
            }

            foreach(var bone in data.Skeleton.Bones)
            {
                writer.Write((byte)0); // Unused flags

                writer.Write(bone.Parent == null ? -1 : data.Skeleton.Bones.IndexOf(bone.Parent));

                var wt = bone.GlobalPosition;
                var wr = bone.GlobalRotation;
                var lt = bone.LocalPosition;
                var lr = bone.LocalRotation;
                var s = Vector3.One;

                writer.Write(wt.X * scale);
                writer.Write(wt.Y * scale);
                writer.Write(wt.Z * scale);
                writer.Write(wr.X);
                writer.Write(wr.Y);
                writer.Write(wr.Z);
                writer.Write(wr.W);

                writer.Write(lt.X * scale);
                writer.Write(lt.Y * scale);
                writer.Write(lt.Z * scale);
                writer.Write(lr.X);
                writer.Write(lr.Y);
                writer.Write(lr.Z);
                writer.Write(lr.W);

                writer.Write(s.X);
                writer.Write(s.Y);
                writer.Write(s.Z);
            }

            foreach (var mesh in data.Meshes)
            {
                var vertCount  = mesh.Positions.Count;
                var faceCount  = mesh.Faces.Count;
                var layerCount = mesh.UVLayers.Dimension > 0 ? 1 : mesh.UVLayers.Dimension;
                var influences = mesh.Influences.Count > 0 ? 0 : mesh.Influences.Dimension;

                writer.Write((byte)0); // Unused flags

                writer.Write((byte)layerCount);
                writer.Write((byte)influences);
                writer.Write(vertCount);
                writer.Write(faceCount);

                // Positions
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    writer.Write(mesh.Positions[i].X * scale);
                    writer.Write(mesh.Positions[i].Y * scale);
                    writer.Write(mesh.Positions[i].Z * scale);
                }
                // UVs
                if(mesh.UVLayers.VertexCount == mesh.Positions.VertexCount && mesh.UVLayers.Dimension > 0)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        for (int l = 0; l < layerCount; l++)
                        {
                            writer.Write(mesh.UVLayers[i, l].X);
                            writer.Write(mesh.UVLayers[i, l].Y);
                        }
                    }
                }
                // Just write 0 values and let the user fix it up in other software.
                else
                {
                    writer.Write(new byte[8 * mesh.Positions.VertexCount * layerCount]);
                }
                // Normals
                if (mesh.Normals.VertexCount == mesh.Positions.VertexCount && mesh.Normals.Dimension > 0)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        writer.Write(mesh.Normals[i].X);
                        writer.Write(mesh.Normals[i].Y);
                        writer.Write(mesh.Normals[i].Z);
                    }
                }
                // Just write 0 values and let the user fix it up in other software.
                else
                {
                    writer.Write(new byte[12 * mesh.Positions.VertexCount]);
                }
                // Colours
                if (mesh.Colours.VertexCount == mesh.Positions.VertexCount && mesh.Colours.Dimension > 0)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        writer.Write((byte)(mesh.Colours[i].X * 255.0f));
                        writer.Write((byte)(mesh.Colours[i].Y * 255.0f));
                        writer.Write((byte)(mesh.Colours[i].Z * 255.0f));
                        writer.Write((byte)(mesh.Colours[i].W * 255.0f));
                    }
                }
                // Just write 0 values and let the user fix it up in other software.
                else
                {
                    writer.Write(new byte[4 * mesh.Positions.VertexCount]);
                }
                // Weights
                if (influences != 0)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        for (int w = 0; w < influences; w++)
                        {
                            var (index, value) = mesh.Influences[i, w];

                            if (data.Skeleton.Bones.Count <= 0xFF)
                                writer.Write((byte)index);
                            else if (data.Skeleton.Bones.Count <= 0xFFFF)
                                writer.Write((ushort)index);
                            else
                                writer.Write(index);

                            writer.Write(value);
                        }
                    }
                }

                foreach (var (firstIndex, secondIndex, thirdIndex) in mesh.Faces)
                {
                    if (vertCount <= 0xFF)
                    {
                        writer.Write((byte)firstIndex);
                        writer.Write((byte)secondIndex);
                        writer.Write((byte)thirdIndex);
                    }
                    else if (vertCount <= 0xFFFF)
                    {
                        writer.Write((ushort)firstIndex);
                        writer.Write((ushort)secondIndex);
                        writer.Write((ushort)thirdIndex);
                    }
                    else
                    {
                        writer.Write(firstIndex);
                        writer.Write(secondIndex);
                        writer.Write(thirdIndex);
                    }
                }

                foreach (var material in mesh.Materials)
                    writer.Write(data.Materials.IndexOf(material));
            }

            foreach (var material in data.Materials)
            {
                writer.Write(Encoding.ASCII.GetBytes(material.Name));
                writer.Write((byte)0);
                writer.Write(true);
                writer.Write(Encoding.ASCII.GetBytes(material.Textures.TryGetValue("DiffuseMap", out var img) ? img.Name : string.Empty));
                writer.Write((byte)0);
                writer.Write(Encoding.ASCII.GetBytes(material.Textures.TryGetValue("NormalMap", out img) ? img.Name : string.Empty));
                writer.Write((byte)0);
                writer.Write(Encoding.ASCII.GetBytes(material.Textures.TryGetValue("SpecularMap", out img) ? img.Name : string.Empty));
                writer.Write((byte)0);
            }
        }

        /// <inheritdoc/>
        public override bool IsValid(Span<byte> startOfFile, Stream stream, string? filePath, string? ext)
        {
            return !string.IsNullOrWhiteSpace(ext) && Extensions.Contains(ext);
        }

        /// <summary>
        /// Reads a UTF8 string from the file
        /// </summary>
        internal static string ReadUTF8String(BinaryReader reader)
        {
            var output = new StringBuilder(32);

            while (true)
            {
                var c = reader.ReadByte();
                if (c == 0)
                    break;
                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }
    }
}