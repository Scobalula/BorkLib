using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    public enum TransformType
    {
        /// <summary>
        /// Type is inherited from the parent.
        /// </summary>
        Parent,

        /// <summary>
        /// Data is relative to the rest pose.
        /// </summary>
        Relative,

        /// <summary>
        /// Data is absolute and overrides the rest position.
        /// </summary>
        Absolute,

        /// <summary>
        /// Data is added onto the current data.
        /// </summary>
        Additive,
    }
}
