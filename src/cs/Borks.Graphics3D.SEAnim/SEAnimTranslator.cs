using System.Numerics;
using System.Text;

namespace Borks.Graphics3D.SEModel
{
    /// <summary>
    /// A class to handle translating data from SEAnim files.
    /// </summary>
    public sealed class SEAnimTranslator : Graphics3DTranslator
    {
        /// <summary>
        /// SEAnim Magic
        /// </summary>
        public static readonly byte[] Magic = { 0x53, 0x45, 0x41, 0x6E, 0x69, 0x6D };

        /// <inheritdoc/>
        public override string Name => "SEAnimTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        {
            ".seanim"
        };

        /// <inheritdoc/>
        public override bool SupportsReading => true;

        /// <inheritdoc/>
        public override bool SupportsWriting => true;

        /// <inheritdoc/>
        public override void Read(Stream stream, string filePath, Graphics3DTranslatorIO output)
        {
            // SEModels can only contain a single skeleton
            // and model.
            var scale = 1.0f;
            var skeleton = new Skeleton();
            var result = new Model(skeleton)
            {
                Name = Path.GetFileNameWithoutExtension(filePath)
            };

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

                var bone = new SkeletonBone(boneNames[i])
                {
                    Index = i
                };

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

                Console.WriteLine(vertexCount);

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
                    mesh.Influences.SetCapacity(vertexCount, influences);

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

            output.Models.Add(result);
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DTranslatorIO input)
        {
            // Determine bones with different types
            var boneModifiers = new Dictionary<int, byte>();

            var data = input.Animations.First();
            var frameCount = data.GetAnimationFrameCount();
            var targetCount = data.SkeletalTargetCount;
            int index = 0;

            if(data.SkeletonAnimation != null)
            {
                var animationType = data.SkeletalTransformType;

                foreach (var bone in data.SkeletonAnimation.Targets)
                {
                    if (bone.TransformType != TransformType.Parent && bone.TransformType != animationType)
                    {
                        // Convert to SEAnim Type
                        switch (bone.TransformType)
                        {
                            case TransformType.Absolute: boneModifiers[index] = 0; break;
                            case TransformType.Additive: boneModifiers[index] = 1; break;
                            case TransformType.Relative: boneModifiers[index] = 2; break;
                        }
                    }

                    index++;
                }
            }

            using var writer = new BinaryWriter(stream);

            writer.Write(Magic);
            writer.Write((ushort)0x1);
            writer.Write((ushort)0x1C);

            // Convert to SEAnim Type
            switch (data.SkeletalTransformType)
            {
                case TransformType.Absolute: writer.Write((byte)1); break;
                case TransformType.Additive: writer.Write((byte)2); break;
                default: writer.Write((byte)0); break;
            }

            writer.Write((byte)0);

            byte flags = 0;

            if (data.HasSkeletalTranslationFrames())
                flags |= 1;
            if (data.HasSkeletalRotationFrames())
                flags |= 2;
            if (data.HasSkeletalScalesFrames())
                flags |= 4;
            //if (data.Notifications.Count > 0)
            //    flags |= 64;

            writer.Write(flags);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write(data.Framerate);
            writer.Write((int)frameCount);
            writer.Write(targetCount);
            writer.Write((byte)boneModifiers.Count);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            // writer.Write(data.GetNotificationFrameCount());
            writer.Write(0);

            if (data.SkeletonAnimation != null)
            {
                var targets = data.SkeletonAnimation.Targets;

                foreach (var bone in targets)
                {
                    writer.Write(Encoding.UTF8.GetBytes(bone.BoneName.Replace('.', '_')));
                    writer.Write((byte)0);
                }

                foreach (var modifier in boneModifiers)
                {
                    if (targetCount <= 0xFF)
                        writer.Write((byte)modifier.Key);
                    else if (targetCount <= 0xFFFF)
                        writer.Write((ushort)modifier.Key);
                    else
                        throw new NotSupportedException();

                    writer.Write(modifier.Value);
                }

                foreach (var bone in targets)
                {
                    writer.Write((byte)0);

                    // TranslationFrames
                    if ((flags & 1) != 0)
                    {
                        if (frameCount <= 0xFF)
                            writer.Write((byte)bone.TranslationFrameCount);
                        else if (frameCount <= 0xFFFF)
                            writer.Write((ushort)bone.TranslationFrameCount);
                        else
                            writer.Write(bone.TranslationFrameCount);

                        if (bone.TranslationFrames != null)
                        {
                            foreach (var frame in bone.TranslationFrames)
                            {
                                if (frameCount <= 0xFF)
                                    writer.Write((byte)frame.Time);
                                else if (frameCount <= 0xFFFF)
                                    writer.Write((ushort)frame.Time);
                                else
                                    writer.Write((int)frame.Time);

                                writer.Write(frame.Value.X * input.Scale);
                                writer.Write(frame.Value.Y * input.Scale);
                                writer.Write(frame.Value.Z * input.Scale);
                            }
                        }
                    }

                    // RotationFrames
                    if ((flags & 2) != 0)
                    {
                        if (frameCount <= 0xFF)
                            writer.Write((byte)bone.RotationFrameCount);
                        else if (frameCount <= 0xFFFF)
                            writer.Write((ushort)bone.RotationFrameCount);
                        else
                            writer.Write(bone.RotationFrameCount);

                        if (bone.RotationFrames != null)
                        {
                            foreach (var frame in bone.RotationFrames)
                            {
                                if (frameCount <= 0xFF)
                                    writer.Write((byte)frame.Time);
                                else if (frameCount <= 0xFFFF)
                                    writer.Write((ushort)frame.Time);
                                else
                                    writer.Write((int)frame.Time);

                                writer.Write(frame.Value.X);
                                writer.Write(frame.Value.Y);
                                writer.Write(frame.Value.Z);
                                writer.Write(frame.Value.W);
                            }
                        }
                    }

                    // ScaleFrames
                    if ((flags & 4) != 0)
                    {
                        if (frameCount <= 0xFF)
                            writer.Write((byte)bone.ScaleFrameCount);
                        else if (frameCount <= 0xFFFF)
                            writer.Write((ushort)bone.ScaleFrameCount);
                        else
                            writer.Write(bone.ScaleFrameCount);

                        if(bone.ScaleFrames != null)
                        {
                            foreach (var frame in bone.ScaleFrames)
                            {
                                if (frameCount <= 0xFF)
                                    writer.Write((byte)frame.Time);
                                else if (frameCount <= 0xFFFF)
                                    writer.Write((ushort)frame.Time);
                                else
                                    writer.Write((int)frame.Time);

                                writer.Write(frame.Value.X);
                                writer.Write(frame.Value.Y);
                                writer.Write(frame.Value.Z);
                            }
                        }
                    }
                }
            }

            //foreach (var note in data.Notifications)
            //{
            //    foreach (var frame in note.Frames)
            //    {
            //        if (frameCount <= 0xFF)
            //            writer.Write((byte)frame.Time);
            //        else if (frameCount <= 0xFFFF)
            //            writer.Write((ushort)frame.Time);
            //        else
            //            writer.Write((int)frame.Time);

            //        writer.Write(Encoding.UTF8.GetBytes(note.Name));
            //        writer.Write((byte)0);
            //    }
            //}
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