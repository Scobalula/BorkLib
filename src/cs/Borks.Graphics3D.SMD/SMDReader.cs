using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.SMD
{
    /// <summary>
    /// A class to handle parsing from Valve SMD files.
    /// </summary>
    internal class SMDReader
    {
        /// <summary>
        /// A comment string in an SMD File
        /// </summary>
        public static char[] CommentString = { '/', '/' };

        /// <summary>
        /// Gets or Sets the file string.
        /// </summary>
        public char[] FileString { get; set; }

        /// <summary>
        /// Gets or Sets the offset within the string that we are currently at.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or Sets the current line number.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Gets or Sets the column.
        /// </summary>
        public int Column { get; set; }

        public SMDReader(TextReader reader)
        {
            FileString = reader.ReadToEnd().ToArray();
        }

        /// <summary>
        /// Skips whitespace just before the token we expect to parse.
        /// </summary>
        /// <returns>Resulting span from the start of the token.</returns>
        public void SkipWhitespace(bool skipNewLines = true)
        {
            while(Offset < FileString.Length)
            {
                if(FileString[Offset] == '\n' && skipNewLines)
                {
                    Offset++;
                    Line++;
                    Column = 0;
                    continue;
                }
                else if(FileString[Offset] != '\r' && FileString[Offset] != ' ' && FileString[Offset] != '\t')
                {
                    break;
                }

                Offset++;
                Column++;
            }
        }

        public void SkipToNextLine()
        {
            while(Offset < FileString.Length)
            {
                if(FileString[Offset] == '\n')
                {
                    Line++;
                    Column = 0;
                    break;
                }

                Offset++;
            }
        }

        public Span<char> Parse(bool skipNewLines = true)
        {
            while (Offset < FileString.Length)
            {
                SkipWhitespace(skipNewLines);
                var startOfToken = Offset;
                var endOfToken = Offset;

                // Second check as we've skipped and could be EOF
                if (Offset >= FileString.Length)
                    break;

                // Check for comments
                if (FileString.AsSpan()[Offset..].StartsWith(CommentString))
                {
                    while(Offset < FileString.Length)
                    {
                        if(FileString[Offset++] == '\n')
                        {
                            Line++;
                            Column = 0;
                            break;
                        }
                    }

                    if (Column != 0)
                    {
                        throw new EndOfStreamException();
                    }

                    continue;
                }
                else if(FileString[Offset] == '"')
                {
                    startOfToken++;
                    endOfToken++;
                    Offset++;
                    Column++;

                    while (Offset < FileString.Length)
                    {
                        if (FileString[Offset] == '\n' ||
                            FileString[Offset] == '\r')
                            throw new SMDReaderException("Unexpected line end in SMD File.", Line, Column);

                        Offset++;
                        Column++;
                        endOfToken++;

                        // Check for string terminator
                        if(FileString[Offset] == '"')
                        {
                            Offset++;
                            Column++;
                            break;
                        }
                    }
                }
                else
                {
                    while (Offset < FileString.Length)
                    {
                        if (FileString[Offset] == '\n' ||
                            FileString[Offset] == '\r' ||
                            FileString[Offset] == '\t' ||
                            FileString[Offset] == ' ')
                            break;

                        Offset++;
                        Column++;
                        endOfToken++;
                    }
                }

                return FileString.AsSpan()[startOfToken..endOfToken];
            }

            return new Span<char>();
        }
    }
}
