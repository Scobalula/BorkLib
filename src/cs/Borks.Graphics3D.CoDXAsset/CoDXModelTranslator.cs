using Borks.Graphics3D.CoDXAsset;
using Borks.Graphics3D.CoDXAsset.Tokens;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Borks.Graphics3D.SEModel
{
    /// <summary>
    /// A class to translate data to a Call of Duty XModel.
    /// </summary>
    public sealed class CoDXModelTranslator : Graphics3DTranslator
    {
        /// <summary>
        /// SEModel Magic
        /// </summary>
        public static readonly byte[] Magic = { 0x53, 0x45, 0x4D, 0x6F, 0x64, 0x65, 0x6C };

        /// <inheritdoc/>
        public override string Name => "CoDXModelTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        {
            ".xmodel_bin",
            ".xmodel_export",
        };

        /// <inheritdoc/>
        public override bool SupportsReading => true;

        /// <inheritdoc/>
        public override bool SupportsWriting => true;

        /// <inheritdoc/>
        public override void Read(Stream stream, string filePath, Graphics3DTranslatorIO output)
        {
            var isBin = filePath.EndsWith(".xmodel_bin", StringComparison.CurrentCultureIgnoreCase);

            TokenReader reader = isBin ? new BinaryTokenReader(stream) : new ExportTokenReader(stream);

            var skeleton = new Skeleton();
            var result = new Model(skeleton)
            {
                Name = Path.GetFileNameWithoutExtension(filePath)
            };

            reader.RequestNextTokenOfType<TokenData>("MODEL");
            reader.RequestNextTokenOfType<TokenDataUInt>("VERSION");


            CoDXModelHelper.ReadBones(reader, skeleton);
            CoDXModelHelper.ReadGeometry(reader, result, skeleton);

            output.Objects.Add(result);
            output.Objects.Add(skeleton);
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DTranslatorIO input)
        {
            var output = input.GetFirstInstance<Model>();

            if (output == null)
                throw new Exception();

            var isBin = filePath.EndsWith(".xmodel_bin", StringComparison.CurrentCultureIgnoreCase);

            using TokenWriter writer = isBin ? new BinaryTokenWriter(stream) : new ExportTokenWriter(stream);

            var skeleton = output.Skeleton;
            var boneCount = skeleton == null ? 0 : skeleton.Bones.Count;

            writer.WriteComment("//", 0xC355, $"Written by Borks - By Scobalula");
            writer.WriteComment("//", 0xC355, $"File: {filePath}");
            writer.WriteComment("//", 0xC355, $"Time: {DateTime.Now}");

            writer.WriteSection("MODEL", 0x46C8);
            writer.WriteUShort("VERSION", 0x24D1, 7);

            // If we have no skeleton, or no bones, we need to write a
            // dummy bone to keep the linker beast calm.
            if(skeleton == null || skeleton.Bones.Count == 0)
            {
                writer.WriteUShort("NUMBONES", 0x76BA, 1);
                writer.WriteBoneInfo("BONE", 0xF099, 0, -1, "TAG_ORIGIN");
                writer.WriteUShort("BONE", 0xDD9A, 0);
                writer.WriteVector3("OFFSET", 0x9383, Vector3.One);
                writer.WriteVector316Bit("X", 0xDCFD, Vector3.UnitX);
                writer.WriteVector316Bit("Y", 0xCCDC, Vector3.UnitY);
                writer.WriteVector316Bit("Z", 0xFCBF, Vector3.UnitZ);
            }
            else
            {
                writer.WriteUShort("NUMBONES", 0x76BA, (ushort)boneCount);

                for (int i = 0; i < skeleton.Bones.Count; i++)
                {
                    var bone = skeleton.Bones[i];
                    var parent = bone.Parent == null ? -1 : skeleton.Bones.IndexOf(bone.Parent);
                    writer.WriteBoneInfo("BONE", 0xF099, i, parent, bone.Name);
                }

                for (int i = 0; i < skeleton.Bones.Count; i++)
                {
                    var bone = skeleton.Bones[i];
                    var translation = bone.BaseWorldTranslation / 2.54f;
                    var matrix = Matrix4x4.CreateFromQuaternion(bone.BaseWorldRotation);

                    writer.WriteUShort("BONE", 0xDD9A, (ushort)i);
                    writer.WriteVector3("OFFSET", 0x9383, translation);
                    writer.WriteVector316Bit("X", 0xDCFD, new(matrix.M11, matrix.M12, matrix.M13));
                    writer.WriteVector316Bit("Y", 0xCCDC, new(matrix.M21, matrix.M22, matrix.M23));
                    writer.WriteVector316Bit("Z", 0xFCBF, new(matrix.M31, matrix.M32, matrix.M33));
                }
            }

            // We could run a check for vert count for bin, but tests have shown with
            // the fact they use LZ4 compression it barely makes a difference
            // on file size......
            writer.WriteUInt("NUMVERTS32", 0x2AEC, (uint)output.GetVertexCount());

            uint vertexIndex = 0;
            for (int m = 0; m < output.Meshes.Count; m++)
            {
                var mesh = output.Meshes[m];
                var influenceCount = mesh.Influences.ElementCount == 0 ? 0 : mesh.Influences.Dimension;

                for (int v = 0; v < mesh.Positions.Count; v++)
                {
                    writer.WriteUInt("VERT32", 0xB097, vertexIndex + (uint)v);
                    writer.WriteVector3("OFFSET", 0x9383, mesh.Positions[v] / 2.54f);
                    writer.WriteUShort("BONES", 0xEA46, (ushort)influenceCount);
                    for (int w = 0; w < influenceCount; w++)
                    {
                        var (index, weight) = mesh.Influences[v, w];
                        writer.WriteBoneWeight("BONE", 0xF1AB, index, weight);
                    }
                }

                vertexIndex += (uint)mesh.Positions.Count;
            }

            writer.WriteUInt("NUMFACES", 0xBE92, (uint)output.GetFaceCount());

            vertexIndex = 0;
            for (int m = 0; m < output.Meshes.Count; m++)
            {
                var mesh = output.Meshes[m];

                var hasNormals  = mesh.Normals.ElementCount == mesh.Positions.ElementCount;
                var hasColors   = mesh.Colours.ElementCount == mesh.Positions.ElementCount;
                var hasUVs      = mesh.UVLayers.ElementCount == mesh.Positions.ElementCount;
                var materialIdx = output.Materials.IndexOf(mesh.Materials[0]);

                for (int f = 0; f < mesh.Faces.Count; f++)
                {
                    var (i0, i1, i2) = mesh.Faces[f];

                    writer.WriteTri16("TRI16", 0x6711, m, materialIdx);

                    writer.WriteUInt("VERT32",          0xB097,          vertexIndex + (uint)i0);
                    writer.WriteVector316Bit("NORMAL",  0x89EC,          hasNormals ? mesh.Normals[i0] : Vector3.One);
                    writer.WriteVector48Bit("COLOR",    0x6DD8,          hasColors ? mesh.Colours[i0] : Vector4.One);
                    writer.WriteUVSet("UV",             0x1AD4,          hasUVs ? mesh.UVLayers[i0, 0] : Vector2.One);

                    writer.WriteUInt("VERT32",          0xB097,          vertexIndex + (uint)i2);
                    writer.WriteVector316Bit("NORMAL",  0x89EC,          hasNormals ? mesh.Normals[i2] : Vector3.One);
                    writer.WriteVector48Bit("COLOR",    0x6DD8,          hasColors ? mesh.Colours[i2] : Vector4.One);
                    writer.WriteUVSet("UV",             0x1AD4,          hasUVs ? mesh.UVLayers[i2, 0] : Vector2.One);

                    writer.WriteUInt("VERT32",          0xB097,          vertexIndex + (uint)i1);
                    writer.WriteVector316Bit("NORMAL",  0x89EC,          hasNormals ? mesh.Normals[i1] : Vector3.One);
                    writer.WriteVector48Bit("COLOR",    0x6DD8,          hasColors ? mesh.Colours[i1] : Vector4.One);
                    writer.WriteUVSet("UV",             0x1AD4,          hasUVs ? mesh.UVLayers[i1, 0] : Vector2.One);
                }

                vertexIndex += (uint)mesh.Positions.Count;
            }

            writer.WriteUShort("NUMOBJECTS", 0x62AF, (ushort)output.Meshes.Count);

            for (int m = 0; m < output.Meshes.Count; m++)
            {
                writer.WriteUShortString("OBJECT", 0x87D4, (ushort)m, $"Mesh_{m}");
            }

            writer.WriteUShort("NUMMATERIALS", 0xA1B2, (ushort)output.Materials.Count);

            for (int m = 0; m < output.Meshes.Count; m++)
            {
                writer.WriteUShortStringX3("MATERIAL", 0xA700, (ushort)m, output.Materials[m].Name, "lambert", "");
                // I don't think these values have EVER been used....maybe in CoD 1 or 2?
                writer.WriteVector48Bit("COLOR", 0x6DD8, Vector4.One);
                writer.WriteVector4("TRANSPARENCY", 0x6DAB, Vector4.One);
                writer.WriteVector4("AMBIENTCOLOR", 0x37FF, Vector4.One);
                writer.WriteVector4("INCANDESCENCE", 0x4265, Vector4.One);
                writer.WriteVector2("COEFFS", 0xC835, Vector2.One);
                writer.WriteVector2("GLOW", 0xFE0C, Vector2.One);
                writer.WriteVector2("REFRACTIVE", 0x7E24, Vector2.One);
                writer.WriteVector4("SPECULARCOLOR", 0x317C, Vector4.One);
                writer.WriteVector4("REFLECTIVECOLOR", 0xE593, Vector4.One);
                writer.WriteVector2("REFLECTIVE", 0x7D76, Vector2.One);
                writer.WriteVector2("BLINN", 0x83C7, Vector2.One);
                writer.WriteFloat("PHONG", 0x5CD2, -1);
            }

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