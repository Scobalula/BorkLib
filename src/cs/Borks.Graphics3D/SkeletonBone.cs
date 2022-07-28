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
        public Vector3 BaseLocalTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the bone rotation relative to its parent.
        /// </summary>
        public Quaternion BaseLocalRotation { get; set; }

        /// <summary>
        /// Gets or Sets the bone position relative to the origin.
        /// </summary>
        public Vector3 BaseWorldTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the bone rotation relative to the origin.
        /// </summary>
        public Quaternion BaseWorldRotation { get; set; }

        /// <summary>
        /// Gets or Sets the current bone position relative to its parent.
        /// </summary>
        public Vector3 CurrentLocalTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the current bone rotation relative to its parent.
        /// </summary>
        public Quaternion CurrentLocalRotation { get; set; }

        /// <summary>
        /// Gets or Sets the current bone position relative to the origin.
        /// </summary>
        public Vector3 CurrentWorldTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the current bone rotation relative to the origin.
        /// </summary>
        public Quaternion CurrentWorldRotation { get; set; }

        /// <summary>
        /// Gets or Sets the scale.
        /// </summary>
        public Vector3 BaseScale { get; set; }

        /// <summary>
        /// Gets or Sets the scale.
        /// </summary>
        public Vector3 CurrentScale { get; set; }

        /// <summary>
        /// Gets or Sets if this bone can be animated.
        /// </summary>
        public bool CanAnimate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public SkeletonBone(string name)
        {
            Name = name;
            Children = new();
            CanAnimate = true;
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
                BaseLocalRotation = Quaternion.Conjugate(Parent.BaseWorldRotation) * BaseWorldRotation;
                BaseLocalTranslation = Vector3.Transform(BaseWorldTranslation - Parent.BaseWorldTranslation, Quaternion.Conjugate(Parent.BaseWorldRotation));
            }
            else
            {
                BaseLocalTranslation = BaseWorldTranslation;
                BaseLocalRotation = BaseWorldRotation;
            }
        }

        public void GenerateWorldTransform()
        {
            if (Parent != null)
            {
                BaseWorldRotation = Parent.BaseWorldRotation * BaseLocalRotation;
                BaseWorldTranslation = Vector3.Transform(BaseLocalTranslation, Parent.BaseWorldRotation) + Parent.BaseWorldTranslation;
            }
            else
            {
                BaseWorldTranslation = BaseLocalTranslation;
                BaseWorldRotation = BaseLocalRotation;
            }
        }

        public void GenerateWorldTransforms()
        {
            GenerateWorldTransform();

            foreach (var child in Children)
            {
                child.GenerateWorldTransforms();
            }
        }

        public void GenerateCurrentLocalTransform()
        {
            if (Parent != null)
            {
                CurrentLocalRotation = Quaternion.Conjugate(Parent.CurrentWorldRotation) * CurrentWorldRotation;
                CurrentLocalTranslation = Vector3.Transform(CurrentWorldTranslation - Parent.CurrentWorldTranslation, Quaternion.Conjugate(Parent.CurrentWorldRotation));
            }
            else
            {
                CurrentLocalTranslation = CurrentWorldTranslation;
                CurrentLocalRotation = CurrentWorldRotation;
            }
        }

        public void GenerateCurrentWorldTransform()
        {
            if (Parent != null)
            {
                CurrentWorldRotation = Parent.CurrentWorldRotation * CurrentLocalRotation;
                CurrentWorldTranslation = Vector3.Transform(CurrentLocalTranslation, Parent.CurrentWorldRotation) + Parent.CurrentWorldTranslation;
            }
            else
            {
                CurrentWorldTranslation = CurrentLocalTranslation;
                CurrentWorldRotation = CurrentLocalRotation;
            }
        }

        public void GenerateCurrentWorldTransforms()
        {
            GenerateCurrentWorldTransform();

            foreach (var child in Children)
            {
                child.GenerateCurrentWorldTransforms();
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

        public void InitializeAnimationTransforms()
        {
            CurrentLocalRotation    = BaseLocalRotation;
            CurrentLocalTranslation = BaseLocalTranslation;
            CurrentWorldTranslation = BaseWorldTranslation;
            CurrentWorldRotation    = BaseWorldRotation;
        }
    }
}
