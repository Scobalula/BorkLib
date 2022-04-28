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
            using var reader = new BinaryReader(stream);

            var magic = reader.ReadChars(6);
            var version = reader.ReadInt16();
            var sizeofHeader = reader.ReadInt16();
            var transformType = TransformType.Unknown;

            switch (reader.ReadByte())
            {
                case 0: transformType = TransformType.Absolute; break;
                case 1: transformType = TransformType.Additive; break;
                case 2: transformType = TransformType.Relative; break;
            }

            var flags         = reader.ReadByte();
            var dataFlags     = reader.ReadByte();
            var dataPropFlags = reader.ReadByte();
            var reserved      = reader.ReadUInt16();
            var frameRate     = reader.ReadSingle();
            var frameCount    = reader.ReadInt32();
            var boneCount     = reader.ReadInt32();
            var modCount      = reader.ReadByte();
            var reserved0     = reader.ReadByte();
            var reserved1     = reader.ReadByte();
            var reserved2     = reader.ReadByte();
            var noteCount     = reader.ReadInt32();

            var anim = new Animation();
            var skelAnim = new SkeletonAnimation(null, boneCount, transformType);

            anim.Framerate = frameRate;

            for (int i = 0; i < boneCount; i++)
            {
                skelAnim.Targets.Add(new(ReadUTF8String(reader)));

                if ((dataFlags & 1) != 0)
                    skelAnim.Targets[i].TranslationFrames = new();
                if ((dataFlags & 2) != 0)
                    skelAnim.Targets[i].RotationFrames = new();
                if ((dataFlags & 4) != 0)
                    skelAnim.Targets[i].ScaleFrames = new();
            }

            for (int i = 0; i < modCount; i++)
            {
                var boneIndex = boneCount <= 0xFF ? reader.ReadByte() : reader.ReadUInt16();

                switch (reader.ReadByte())
                {
                    case 0: skelAnim.Targets[boneIndex].ChildTransformType = TransformType.Absolute; break;
                    case 1: skelAnim.Targets[boneIndex].ChildTransformType = TransformType.Additive; break;
                    case 2: skelAnim.Targets[boneIndex].ChildTransformType = TransformType.Relative; break;
                }
            }

            foreach (var bone in skelAnim.Targets)
            {
                reader.ReadByte();

                if ((dataFlags & 1) != 0)
                {
                    int keyCount;

                    if (frameCount <= 0xFF)
                        keyCount = reader.ReadByte();
                    else if (frameCount <= 0xFFFF)
                        keyCount = reader.ReadUInt16();
                    else
                        keyCount = reader.ReadInt32();

                    for (int f = 0; f < keyCount; f++)
                    {
                        int frame;

                        if (frameCount <= 0xFF)
                            frame = reader.ReadByte();
                        else if (frameCount <= 0xFFFF)
                            frame = reader.ReadUInt16();
                        else
                            frame = reader.ReadInt32();

                        if ((dataPropFlags & (1 << 0)) == 0)
                            bone.TranslationFrames!.Add(new(
                                frame,
                                new(reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle())));
                        else
                            bone.TranslationFrames!.Add(new(
                                frame,
                                new((float)reader.ReadDouble(),
                                    (float)reader.ReadDouble(),
                                    (float)reader.ReadDouble())));
                    }
                }

                if ((dataFlags & 2) != 0)
                {
                    int keyCount;

                    if (frameCount <= 0xFF)
                        keyCount = reader.ReadByte();
                    else if (frameCount <= 0xFFFF)
                        keyCount = reader.ReadUInt16();
                    else
                        keyCount = reader.ReadInt32();

                    for (int f = 0; f < keyCount; f++)
                    {
                        int frame;

                        if (frameCount <= 0xFF)
                            frame = reader.ReadByte();
                        else if (frameCount <= 0xFFFF)
                            frame = reader.ReadUInt16();
                        else
                            frame = reader.ReadInt32();

                        if ((dataPropFlags & (1 << 0)) == 0)
                            bone.RotationFrames!.Add(new(
                                frame, 
                                new(reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle())));
                        else
                            bone.RotationFrames!.Add(new(
                                frame,
                                new((float)reader.ReadDouble(),
                                    (float)reader.ReadDouble(),
                                    (float)reader.ReadDouble(),
                                    (float)reader.ReadDouble())));
                    }
                }

                if ((dataFlags & 4) != 0)
                {
                    int keyCount;

                    if (frameCount <= 0xFF)
                        keyCount = reader.ReadByte();
                    else if (frameCount <= 0xFFFF)
                        keyCount = reader.ReadUInt16();
                    else
                        keyCount = reader.ReadInt32();

                    for (int f = 0; f < keyCount; f++)
                    {
                        int frame;

                        if (frameCount <= 0xFF)
                            frame = reader.ReadByte();
                        else if (frameCount <= 0xFFFF)
                            frame = reader.ReadUInt16();
                        else
                            frame = reader.ReadInt32();

                        if ((dataPropFlags & (1 << 0)) == 0)
                            bone.ScaleFrames!.Add(new(
                                frame,
                                new(reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle())));
                        else
                            bone.ScaleFrames!.Add(new(
                                frame,
                                new((float)reader.ReadDouble(),
                                    (float)reader.ReadDouble(),
                                    (float)reader.ReadDouble())));
                    }
                }
            }

            for (int i = 0; i < noteCount; i++)
            {
                int frame;

                if (frameCount <= 0xFF)
                    frame = reader.ReadByte();
                else if (frameCount <= 0xFFFF)
                    frame = reader.ReadUInt16();
                else
                    frame = reader.ReadInt32();

                anim.CreateAction(ReadUTF8String(reader)).Frames.Add(new(frame, null));
            }

            anim.SkeletonAnimation = skelAnim;

            output.Animations.Add(anim);
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DTranslatorIO input)
        {
            // Determine bones with different types
            var boneModifiers = new Dictionary<int, byte>();

            var data        = input.Animations.First();
            var frameCount  = data.GetAnimationFrameCount();
            var actionCount = data.GetAnimationActionCount();
            var targetCount = data.SkeletalTargetCount;
            int index       = 0;

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
                case TransformType.Absolute: writer.Write((byte)0); break;
                case TransformType.Additive: writer.Write((byte)1); break;
                default: writer.Write((byte)2); break;
            }

            writer.Write((byte)0);

            byte flags = 0;

            if (data.HasSkeletalTranslationFrames())
                flags |= 1;
            if (data.HasSkeletalRotationFrames())
                flags |= 2;
            if (data.HasSkeletalScalesFrames())
                flags |= 4;
            if (actionCount > 0)
                flags |= 64;

            writer.Write(flags);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write(data.Framerate);
            writer.Write((int)frameCount);
            writer.Write(targetCount);
            writer.Write((byte)boneModifiers.Count);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write(actionCount);

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

            foreach (var action in data.Actions)
            {
                foreach (var frame in action.Frames)
                {
                    if (frameCount <= 0xFF)
                        writer.Write((byte)frame.Time);
                    else if (frameCount <= 0xFFFF)
                        writer.Write((ushort)frame.Time);
                    else
                        writer.Write((int)frame.Time);

                    writer.Write(Encoding.UTF8.GetBytes(action.Name));
                    writer.Write((byte)0);
                }
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