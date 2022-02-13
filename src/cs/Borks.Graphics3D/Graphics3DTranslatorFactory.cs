using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to handle obtaining valid a <see cref="Graphics3DTranslator"/> given arbitrary data.
    /// </summary>
    public class Graphics3DTranslatorFactory
    {
        /// <summary>
        /// Gets the supported translators.
        /// </summary>
        private List<Graphics3DTranslator> Translators { get; } = new();

        /// <summary>
        /// Registers the provided translator for use in file translation.
        /// </summary>
        /// <param name="translator">The translator to register.</param>
        public void RegisterTranslator(Graphics3DTranslator translator)
        {
            Translators.Add(translator);
        }

        /// <summary>
        /// Attempts to read from the provided file.
        /// </summary>
        /// <param name="filePath">Full file path.</param>
        /// <param name="io">The results from the translator.</param>
        /// <returns>True if a valid translator was found, otherwise false.</returns>
        public bool TryLoadFile(string filePath, Graphics3DTranslatorIO io)
        {
            using var stream = File.OpenRead(filePath);
            return TryLoadStream(stream, filePath, io);
        }

        /// <summary>
        /// Attempts to read from the provided stream.
        /// </summary>
        /// <param name="stream">The stream that contains the data.</param>
        /// <param name="filePath">Full file path.</param>
        /// <param name="io">The results from the translator.</param>
        /// <returns>True if a valid translator was found, otherwise false.</returns>
        public bool TryLoadStream(Stream stream, string filePath, Graphics3DTranslatorIO io)
        {
            if(TryGetTranslator(stream, filePath, out var translator) && translator.SupportsReading)
            {
                translator.Read(stream, filePath, io);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">Full file path.</param>
        /// <param name="io"></param>
        /// <returns>True if a valid translator was found, otherwise false.</returns>
        public bool TrySaveFile(string filePath, Graphics3DTranslatorIO io)
        {
            using var stream = File.Create(filePath);
            return TrySaveStream(stream, filePath, io);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filePath">Full file path.</param>
        /// <param name="io"></param>
        /// <returns>True if a valid translator was found, otherwise false.</returns>
        public bool TrySaveStream(Stream stream, string filePath, Graphics3DTranslatorIO io)
        {
            if (TryGetTranslator(null, filePath, out var translator) && translator.SupportsReading)
            {
                translator.Write(stream, filePath, io);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to get the translator for the given stream with the given file name.
        /// </summary>
        /// <param name="stream">The stream that contains the data, if null, only the file name is used for checks.</param>
        /// <param name="filePath">Full file path.</param>
        /// <param name="translator">The resulting translator for the given stream, if not found, this will be null</param>
        /// <returns>True if found, otherwise false.</returns>
        public bool TryGetTranslator(Stream? stream, string filePath,[NotNullWhen(true)] out Graphics3DTranslator? translator)
        {
            var extension = Path.GetExtension(filePath);

            if(stream != null)
            {
                Span<byte> buffer = stackalloc byte[512];
                stream.Read(buffer);
                stream.Seek(0, SeekOrigin.Begin);

                foreach (var potentialTranslator in Translators)
                {
                    if (potentialTranslator.IsValid(buffer, stream, filePath, extension))
                    {
                        translator = potentialTranslator;
                        return true;
                    }
                }
            }
            else
            {
                foreach (var potentialTranslator in Translators)
                {
                    if (potentialTranslator.IsValid(filePath, extension))
                    {
                        translator = potentialTranslator;
                        return true;
                    }
                }
            }

            translator = null;
            return false;
        }
    }
}
