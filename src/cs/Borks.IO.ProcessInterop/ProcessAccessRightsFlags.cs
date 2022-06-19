using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.IO.ProcessInterop
{
    /// <summary>
    /// Process Stream Access Rights
    /// </summary>
    [Flags]
    public enum ProcessAccessRightsFlags
    {
        /// <summary>
        /// Open the Process with Read Access
        /// </summary>
        Read = 0x0010,

        /// <summary>
        /// Open the Process with Write Access
        /// </summary>
        Write = 0x0020,
    }
}
