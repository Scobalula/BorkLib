using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class to hold a bone that is contained within a <see cref="Skeleton"/> for use in Skeletal Animation, etc.
    /// </summary>
    public class SkeletonBone
    {
        /// <summary>
        /// Internal bone parent value.
        /// </summary>
        private SkeletonBone? _parent;

        /// <summary>
        /// Gets or Sets the name of the bone.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the parent of this bone.
        /// </summary>
        public SkeletonBone? Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent?.Children.Remove(this);
                _parent = value;
                _parent?.Children.Add(this);
            }
        }

        /// <summary>
        /// Gets or Sets the bones that are children of this bone.
        /// </summary>
        public List<SkeletonBone> Children { get; set; }

        /// <summary>
        /// Gets or Sets the bone position relative to its parent.
        /// </summary>
        public Vector3 LocalPosition { get; set; }

        /// <summary>
        /// Gets or Sets the bone rotation relative to its parent.
        /// </summary>
        public Quaternion LocalRotation { get; set; }

        /// <summary>
        /// Gets or Sets the bone position relative to the origin.
        /// </summary>
        public Vector3 GlobalPosition { get; set; }

        /// <summary>
        /// Gets or Sets the bone rotation relative to the origin.
        /// </summary>
        public Quaternion GlobalRotation { get; set; }

        /// <summary>
        /// Gets or Sets the scale.
        /// </summary>
        public Vector3 Scale { get; set; }

        public SkeletonBone(string name)
        {
            Name = name;
            Children = new();
        }
    }
}
