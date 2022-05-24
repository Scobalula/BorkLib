using Borks.Graphics3D.CoDXAsset;
using Borks.Graphics3D.CoDXAsset.Tokens;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Borks.Graphics3D.SEModel
{
    /// <summary>
    /// A class to translate a Model to SEModel
    /// </summary>
    public sealed class CoDXAnimTranslator : Graphics3DTranslator
    {
        /// <summary>
        /// SEModel Magic
        /// </summary>
        public static readonly byte[] Magic = { 0x53, 0x45, 0x4D, 0x6F, 0x64, 0x65, 0x6C };

        /// <inheritdoc/>
        public override string Name => "CoDXAssetTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        {
            ".xanim_bin",
            ".xanim_export",
        };

        /// <inheritdoc/>
        public override bool SupportsReading => true;

        /// <inheritdoc/>
        public override bool SupportsWriting => true;

        /// <inheritdoc/>
        public override void Read(Stream stream, string filePath, Graphics3DTranslatorIO output)
        {
            var isBin = filePath.EndsWith(".xanim_bin", StringComparison.CurrentCultureIgnoreCase);

            TokenReader reader = isBin ? new BinaryTokenReader(stream) : new ExportTokenReader(stream);

            reader.RequestNextTokenOfType<TokenData>("ANIMATION");
            reader.RequestNextTokenOfType<TokenDataUInt>("VERSION");

            var partCount = reader.RequestNextTokenOfType<TokenDataUInt>("NUMPARTS");

            var skeletonAnimation = new SkeletonAnimation(null, (int)partCount.Data, TransformType.Absolute)
            {
                TransformSpace = TransformSpace.World,
            };
            var result = new Animation()
            {
                SkeletonAnimation = skeletonAnimation
            };

            for (int i = 0; i < partCount.Data; i++)
            {
                var part = reader.RequestNextTokenOfType<TokenDataUIntString>("PART");

                if (part.IntegerValue != i)
                {
                    throw new InvalidDataException($"Part index for: {part.StringValue} does not match current index.");
                }

                skeletonAnimation.Targets.Add(new(part.StringValue.ToLower())
                {
                    TranslationFrames = new(),
                    RotationFrames = new()
                });
            }

            var frameRate = reader.RequestNextTokenOfType<TokenDataUInt>("FRAMERATE");
            var frameCount = reader.RequestNextTokenOfType<TokenDataUInt>("NUMFRAMES");

            result.Framerate = frameRate.Data;

            for (int i = 0; i < frameCount.Data; i++)
            {
                var frame = reader.RequestNextTokenOfType<TokenDataUInt>("FRAME").Data;

                for (int p = 0; p < partCount.Data; p++)
                {
                    var part = (int)reader.RequestNextTokenOfType<TokenDataUInt>("PART").Data;
                    var offset = reader.RequestNextTokenOfType<TokenDataVector3>("OFFSET").Data;
                    var xRow = reader.RequestNextTokenOfType<TokenDataVector3>("X").Data;
                    var yRow = reader.RequestNextTokenOfType<TokenDataVector3>("Y").Data;
                    var zRow = reader.RequestNextTokenOfType<TokenDataVector3>("Z").Data;

                    var matrix = new Matrix4x4(
                        xRow.X, xRow.Y, xRow.Z, 0,
                        yRow.X, yRow.Y, yRow.Z, 0,
                        zRow.X, zRow.Y, zRow.Z, 0,
                        0, 0, 0, 1
                        );

                    skeletonAnimation.Targets[part].TranslationFrames!.Add(new(frame, offset * 2.54f));
                    skeletonAnimation.Targets[part].RotationFrames!.Add(new(frame, Quaternion.CreateFromRotationMatrix(matrix)));
                }
            }

            output.Objects.Add(result);
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DTranslatorIO input)
        {
            var anim = input.GetFirstInstance<Animation>();

            if (anim == null)
                throw new Exception();
            if(anim.SkeletonAnimation == null)
                throw new Exception();

            var numTargets = anim.SkeletonAnimation.Targets.Count;
            var isBin = filePath.EndsWith(".xanim_bin", StringComparison.CurrentCultureIgnoreCase);

            using TokenWriter writer = isBin ? new BinaryTokenWriter(stream) : new ExportTokenWriter(stream);

            writer.WriteComment("//", 0xC355, $"Written by Borks - By Scobalula");
            writer.WriteComment("//", 0xC355, $"File: {filePath}");
            writer.WriteComment("//", 0xC355, $"Time: {DateTime.Now}");

            writer.WriteSection("ANIMATION", 0x7AAC);
            writer.WriteUShort("VERSION", 0x24D1, 3);

            writer.WriteUShort("NUMPARTS", 0x9279, (ushort)anim.SkeletonAnimation.Targets.Count);


            var idx = 0;
            foreach (var part in anim.SkeletonAnimation.Targets)
            {
                writer.WriteUShortString("PART", 0x360B, (ushort)idx++, part.BoneName);
            }

            var frameCount = anim.GetAnimationFrameCount();

            writer.WriteUShort("FRAMERATE", 0x92D3, (ushort)anim.Framerate);
            writer.WriteUInt("NUMFRAMES", 0xB917, (uint)frameCount);


            for (uint i = 0; i < frameCount; i++)
            {
                writer.WriteUInt("FRAME", 0xC723, i);

                for (int p = 0; p < numTargets; p++)
                {
                    var target = anim.SkeletonAnimation.Targets[p];

                    if (target.TranslationFrameCount > 0 && target.RotationFrameCount > 0)
                    {
                        var translation = target.SampleTranslation(i) / 2.54f;
                        var rotation    = target.SampleRotation(i);
                        var matrix      = Matrix4x4.CreateFromQuaternion(rotation);

                        writer.WriteUShort("PART", 0x745A, (ushort)p);
                        writer.WriteVector3("OFFSET", 0x9383, translation);
                        writer.WriteVector316Bit("X", 0xDCFD, new(matrix.M11, matrix.M12, matrix.M13));
                        writer.WriteVector316Bit("Y", 0xCCDC, new(matrix.M21, matrix.M22, matrix.M23));
                        writer.WriteVector316Bit("Z", 0xFCBF, new(matrix.M31, matrix.M32, matrix.M33));
                    }
                }
            }

            writer.WriteUShort("NUMKEYS", 0x7A6C, 0);
            writer.FinalizeWrite();
        }

        /// <inheritdoc/>
        public override bool IsValid(Span<byte> startOfFile, Stream stream, string? filePath, string? ext)
        {
            return !string.IsNullOrWhiteSpace(ext) && Extensions.Contains(ext, StringComparer.CurrentCultureIgnoreCase);
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