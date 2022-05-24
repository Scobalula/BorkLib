using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.CoDXAsset.Tokens
{
    public sealed class BinaryTokenReader : TokenReader
    {
        /// <summary>
        /// Gets the Reader
        /// </summary>
        internal BinaryReader Reader { get; set; }

        static byte[] Decompress(Stream stream)
        {
            Span<byte> magicBuf = stackalloc byte[5];
            Span<byte> sizeBuf = stackalloc byte[4];

            if (stream.Read(magicBuf) < magicBuf.Length)
                throw new IOException("Unexpected end of file while reading XBin Magic");
            if (stream.Read(sizeBuf) < sizeBuf.Length)
                throw new IOException("Unexpected end of file while reading XBin Size");

            var compressedSize = stream.Length - stream.Position;
            var size = BitConverter.ToInt32(sizeBuf);

            var compressedBuffer = new byte[compressedSize];
            var decompressedBuffer = new byte[size];

            if (stream.Read(compressedBuffer) < compressedBuffer.Length)
                throw new IOException("Unexpected end of file while reading XBin Data");
            if (LZ4Codec.Decode(compressedBuffer, decompressedBuffer) != size)
                throw new IOException("No more kitten, daddy is not ready.");

            File.WriteAllBytes("test.dat", decompressedBuffer);

            return decompressedBuffer;
        }

        public BinaryTokenReader(string fileName)
        {
            using var temp = File.OpenRead(fileName);
            Reader = new(new MemoryStream(Decompress(temp)), Encoding.Default, true);
        }

        public BinaryTokenReader(Stream stream)
        {
            Reader = new(new MemoryStream(Decompress(stream)), Encoding.Default, true);
        }

        internal void AlignReader(long alignment)
        {
            alignment -= 1;
            Reader.BaseStream.Position = ~alignment & Reader.BaseStream.Position + alignment;
        }

        internal string ReadUTF8String()
        {
            var output = new StringBuilder(32);

            while (true)
            {
                var c = Reader.ReadByte();
                if (c == 0)
                    break;
                output.Append(Convert.ToChar(c));
            }

            AlignReader(4);

            return output.ToString();
        }

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public override TokenData? RequestNextToken()
        {
            // No tokens left
            if (Reader.BaseStream.Position >= Reader.BaseStream.Length)
                return null;

            AlignReader(4);

            if (!Token.TryGetToken(Reader.ReadUInt16(), out var token))
                throw new IOException("Unrecognized Token");

            switch(token.DataType)
            {
                case TokenDataType.Comment:
                    {
                        AlignReader(4);
                        ReadUTF8String();
                        return new TokenData(token);
                    }
                case TokenDataType.Section:
                    {
                        return new TokenData(token);
                    }
                case TokenDataType.BoneInfo:
                    {
                        AlignReader(4);
                        return new TokenDataBoneInfo(
                            Reader.ReadInt32(),
                            Reader.ReadInt32(),
                            ReadUTF8String(), token);
                    }
                case TokenDataType.Short:
                    {
                        AlignReader(2);
                        return new TokenDataInt(Reader.ReadInt16(), token);
                    }
                case TokenDataType.UShortString:
                    {
                        AlignReader(2);
                        return new TokenDataUIntString(
                            Reader.ReadUInt16(),
                            ReadUTF8String(), token);
                    }
                case TokenDataType.UShortStringX3:
                    {
                        AlignReader(2);
                        return new TokenDataUIntStringX3(
                            Reader.ReadUInt16(),
                            ReadUTF8String(),
                            ReadUTF8String(),
                            ReadUTF8String(), token);
                    }
                case TokenDataType.UShort:
                    {
                        AlignReader(2);
                        return new TokenDataUInt(Reader.ReadUInt16(), token);
                    }
                case TokenDataType.Int:
                    {
                        AlignReader(4);
                        return new TokenDataInt(Reader.ReadInt32(), token);
                    }
                case TokenDataType.UInt:
                    {
                        AlignReader(4);
                        return new TokenDataUInt(Reader.ReadUInt32(), token);
                    }
                case TokenDataType.Float:
                    {
                        AlignReader(4);
                        return new TokenDataFloat(Reader.ReadSingle(), token);
                    }
                case TokenDataType.Vector2:
                    {
                        AlignReader(4);
                        return new TokenDataVector2(new(
                            Reader.ReadSingle(),
                            Reader.ReadSingle()), token);
                    }
                case TokenDataType.Vector3:
                    {
                        AlignReader(4);
                        return new TokenDataVector3(new(
                            Reader.ReadSingle(),
                            Reader.ReadSingle(),
                            Reader.ReadSingle()), token);
                    }
                case TokenDataType.Vector316Bit:
                    {
                        AlignReader(2);
                        return new TokenDataVector3(new(
                            Reader.ReadInt16() * (1 / 32767.0f),
                            Reader.ReadInt16() * (1 / 32767.0f),
                            Reader.ReadInt16() * (1 / 32767.0f)), token);
                    }
                case TokenDataType.Vector4:
                    {
                        AlignReader(4);
                        return new TokenDataVector4(new(
                            Reader.ReadSingle(),
                            Reader.ReadSingle(),
                            Reader.ReadSingle(),
                            Reader.ReadSingle()), token);
                    }
                case TokenDataType.Vector48Bit:
                    {
                        AlignReader(4);
                        return new TokenDataVector4(new(
                            Reader.ReadByte() * (1 / 255.0f),
                            Reader.ReadByte() * (1 / 255.0f),
                            Reader.ReadByte() * (1 / 255.0f),
                            Reader.ReadByte() * (1 / 255.0f)), token);
                    }

                case TokenDataType.BoneWeight:
                    {
                        AlignReader(2);
                        return new TokenDataBoneWeight(
                            Reader.ReadUInt16(),
                            Reader.ReadSingle(), token);
                    }
                case TokenDataType.Tri:
                    {
                        return new TokenDataTri(
                            Reader.ReadByte(),
                            Reader.ReadByte(), token);
                    }
                case TokenDataType.UVSet:
                    {
                        var result = new TokenDataUVSet(token);
                        var uvSets = Reader.ReadUInt16();
                        for (int i = 0; i < uvSets; i++)
                            result.UVs.Add(new(
                                Reader.ReadSingle(),
                                Reader.ReadSingle()));
                        return result;
                    }
            }

            throw new IOException($"Token Data Type: {token.DataType} for Token: {token.Name} @ {Reader.BaseStream.Position}");
        }

        /// <summary>
        /// Finalizes the write, performing any necessary compression, flushing, etc.
        /// </summary>
        public override void FinalizeRead()
        {

        }

        /// <summary>
        /// Disposes of the data
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Reader != null)
                {
                    Reader.Dispose();
                }
            }
        }
    }
}
