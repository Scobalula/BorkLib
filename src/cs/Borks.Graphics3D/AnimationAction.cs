using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold an animation action that executes an action during animation playback.
    /// </summary>
    public class AnimationAction
    {
        /// <summary>
        /// Gets or Sets the name of the action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the type of the action.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets the frames this action occurs at.
        /// </summary>
        public List<AnimationFrame<Action<AnimationAction>?>> Frames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationAction"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public AnimationAction(string name, string type)
        {
            Name = name;
            Type = type;
            Frames = new();
        }
    }
}
