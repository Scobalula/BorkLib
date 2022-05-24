using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.Translator
{
    /// <summary>
    /// An exception thrown if an unknown file format is found.
    /// </summary>
    public class Unknown3DFileFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Unknown3DFileFormatException"/> class.
        /// </summary>
        public Unknown3DFileFormatException() : base() { }
    }
}
