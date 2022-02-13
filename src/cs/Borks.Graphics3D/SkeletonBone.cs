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
        /// Gets or Sets the index of the bone within the <see cref="Skeleton"/>.
        /// </summary>
        public int Index { get; set; }

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

        /// <summary>
        /// Checks if this bone is a descendant of the given bone.
        /// </summary>
        /// <param name="bone">Parent to check for.</param>
        /// <returns>True if it is, otherwise false.</returns>
        public bool IsDescendantOf(SkeletonBone? bone)
        {
            if (bone == null)
                return false;

            var current = Parent;

            while (current is not null)
            {
                if (current == bone)
                    return true;

                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Checks if this bone is a descendant of the given bone by name.
        /// </summary>
        /// <param name="boneName">Name to check for.</param>
        /// <returns>True if it is, otherwise false.</returns>
        public bool IsDescendantOf(string? boneName) =>
            IsDescendantOf(boneName, StringComparison.CurrentCulture);

        /// <summary>
        /// Checks if this bone is a descendant of the given bone by name.
        /// </summary>
        /// <param name="boneName">Name to check for.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared.</param>
        /// <returns>True if it is, otherwise false.</returns>
        public bool IsDescendantOf(string? boneName, StringComparison comparisonType)
        {
            if (string.IsNullOrWhiteSpace(boneName))
                return false;
            var current = Parent;

            while (current is not null)
            {
                if (current.Name.Equals(boneName, comparisonType))
                    return true;

                current = current.Parent;
            }

            return false;
        }

        public void GenerateLocalTransform()
        {
            if (Parent != null)
            {
                LocalRotation = Quaternion.Conjugate(Parent.GlobalRotation) * GlobalRotation;
                LocalPosition = Vector3.Transform(GlobalPosition - Parent.GlobalPosition, Quaternion.Conjugate(Parent.GlobalRotation));
            }
            else
            {
                LocalPosition = GlobalPosition;
                LocalRotation = GlobalRotation;
            }
        }

        public void GenerateGlobalTransform()
        {
            if (Parent != null)
            {
                GlobalRotation = Parent.GlobalRotation * LocalRotation;
                GlobalPosition = Vector3.Transform(LocalPosition, Parent.GlobalRotation) + Parent.GlobalPosition;
            }
            else
            {
                GlobalPosition = LocalPosition;
                GlobalRotation = LocalRotation;
            }
        }

        public IEnumerable<SkeletonBone> EnumerateParents()
        {
            var parent = Parent;

            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }
    }
}
