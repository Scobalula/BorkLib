using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.SMD
{
    /// <summary>
    /// Represents errors that occur during the use of the <see cref="SMDReader"/> class.
    /// </summary>
    public class SMDReaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SMDReaderException"/> class.
        /// </summary>
        public SMDReaderException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMDReaderException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="line">The line that the error occured at within the SMD file.</param>
        /// <param name="col">The column that the error occured at within the SMD file.</param>
        public SMDReaderException(string message, int line, int col) : base($"Message: {message} Line: {line} Column: {col}") { }
    }
}
