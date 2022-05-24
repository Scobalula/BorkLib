using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D.Translator
{
    /// <summary>
    /// An exception thrown if an empty file is found.
    /// </summary>
    public class Empty3DFileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Empty3DFileException"/> class.
        /// </summary>
        public Empty3DFileException() : base() { }
    }
}
