using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class Graphics3DTranslator
    {
        /// <summary>
        /// Gets the name of the translator.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets whether or not this translator supports reading.
        /// </summary>
        public abstract bool SupportsReading { get; }

        /// <summary>
        /// Gets whether or not this translator supports writing.
        /// </summary>
        public abstract bool SupportsWriting { get; }

        /// <summary>
        /// Gets the file extensions this translator uses.
        /// </summary>
        public abstract string[] Extensions { get; }

        /// <summary>
        /// Translates data stored within the file and returns any valid data read.
        /// </summary>
        /// <param name="filePath">Full file path to load from.</param>
        /// <param name="output">The <see cref="Graphics3DTranslatorIO"/> where to store any results read from the file.</param>
        public virtual void Read(string filePath, Graphics3DTranslatorIO output)
        {
            using var stream = File.OpenRead(filePath);
            Read(stream, filePath, output);
        }

        /// <summary>
        /// Translates data stored within the stream and returns any valid data read.
        /// </summary>
        /// <param name="stream">Stream to load the data from.</param>
        /// <param name="filePath">Full file path.</param>
        /// <param name="output">The <see cref="Graphics3DTranslatorIO"/> where to store any results read from the file.</param>
        public abstract void Read(Stream stream, string fileName, Graphics3DTranslatorIO output);

        /// <summary>
        /// Translates the data to the provided file.
        /// </summary>
        /// <param name="filePath">Full file path to write to.</param>
        /// <param name="input">The <see cref="Graphics3DTranslatorIO"/> that contains the data we are writing.</param>
        public virtual void Write(string filePath, Graphics3DTranslatorIO input)
        {
            using var stream = File.Create(filePath);
            Write(stream, filePath, input);
        }

        /// <summary>
        /// Translates the data to the provided stream.
        /// </summary>
        /// <param name="stream">Stream to write the data to.</param>
        /// <param name="filePath">Full file path.</param>
        /// <param name="input">The <see cref="Graphics3DTranslatorIO"/> that contains the data we are writing.</param>
        public abstract void Write(Stream stream, string filePath, Graphics3DTranslatorIO input);

        /// <summary>
        /// Checks if the provided input is supported by this translator.
        /// </summary>
        /// <param name="filePath">Full file path.</param>
        /// <param name="ext">File extension.</param>
        /// <returns>True if supported by this translator, otherwise false.</returns>
        public virtual bool IsValid(string? filePath, string? ext) =>
            Extensions.Contains(ext);

        /// <summary>
        /// Checks if the provided input is supported by this translator.
        /// </summary>
        /// <param name="startOfFile">An initial buffer from the start of the file.</param>
        /// <param name="stream">Stream we are attempting to load.</param>
        /// <param name="filePath">Full file path.</param>
        /// <param name="ext">File extension.</param>
        /// <returns>True if supported by this translator, otherwise false.</returns>
        public virtual bool IsValid(Span<byte> startOfFile, Stream stream, string? filePath, string? ext) =>
            IsValid(filePath, ext);
    }
}
