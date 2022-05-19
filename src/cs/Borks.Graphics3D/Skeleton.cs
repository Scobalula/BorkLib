using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// A class that defines 
    /// </summary>
    public class Skeleton : Graphics3DObject
    {
        /// <summary>
        /// Gets or Sets the bones stored within this skeleton.
        /// </summary>
        public List<SkeletonBone> Bones { get; set; }

        public Skeleton()
        {
            Bones = new();
        }

        public SkeletonBone? GetFirstRoot()
        {
            foreach (var bone in Bones)
            {
                if(bone.Parent == null)
                {
                    return bone;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the bone by name.
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>Bone if found, otherwise null.</returns>
        public SkeletonBone? GetBone(string? boneName) => Bones.Find(x => x.Name.Equals(boneName));

        /// <summary>
        /// Gets the bone index by name.
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>Index of the bone if found, otherwise -1</returns>
        public int GetBoneIndex(string? boneName) => Bones.FindIndex(x => x.Name.Equals(boneName));

        /// <summary>
        /// Attempts to get the bone by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBone(string? boneName, out int boneIndex, [NotNullWhen(true)] out SkeletonBone? bone)
        {
            if(TryGetBoneIndex(boneName, out boneIndex))
            {
                bone = Bones[boneIndex];
                return true;
            }

            boneIndex = -1;
            bone = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the bone by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBone(string? boneName, [NotNullWhen(true)] out SkeletonBone? bone) => (bone = Bones.Find(x => x.Name.Equals(boneName))) != null;

        /// <summary>
        /// Attempts to get the bone index by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBoneIndex(string? boneName, out int boneIndex) => (boneIndex = Bones.FindIndex(x => x.Name.Equals(boneName))) != -1;

        /// <summary>
        /// Determines whether the model contains the bone
        /// /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool ContainsBone(string? boneName) => Bones.FindIndex(x => x.Name.Equals(boneName)) != -1;

        /// <summary>
        /// Enumerates the bones depth first from the root nodes.
        /// </summary>
        /// <returns>Current bone.</returns>
        public IEnumerable<SkeletonBone> EnumerateBones()
        {
            var boneStack = new Stack<SkeletonBone>();

            // Push roots first
            foreach (var bone in Bones)
                if (bone.Parent == null)
                    boneStack.Push(bone);

            while (boneStack.Count > 0)
            {
                var currentBone = boneStack.Pop();

                yield return currentBone;

                foreach (var bone in currentBone.Children)
                    boneStack.Push(bone);
            }
        }

        public void GenerateLocalTransforms()
        {
            foreach (var bone in EnumerateBones())
            {
                bone.GenerateLocalTransform();
            }
        }

        public void GenerateGlobalTransforms()
        {
            foreach (var bone in EnumerateBones())
            {
                bone.GenerateWorldTransform();
            }
        }

        /// <summary>
        /// Assigns the bone indices based off their index within the table. 
        /// </summary>
        public void AssignBoneIndices()
        {
            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i].Index = i;
            }
        }

        public Skeleton CreateCopy()
        {
            var newSkeleton = new Skeleton();
            var bones = new SkeletonBone[Bones.Count];

            foreach(var bone in EnumerateBones())
            {
                newSkeleton.Bones[bone.Index] = new SkeletonBone(bone.Name)
                {
                    Index                = bone.Index,
                    Parent               = bone.Parent != null ? bones.First(x => x.Name == bone.Parent?.Name) : null,
                    BaseLocalTranslation = bone.BaseLocalTranslation,
                    BaseLocalRotation    = bone.BaseLocalRotation,
                    BaseWorldTranslation = bone.BaseWorldTranslation,
                    BaseWorldRotation    = bone.BaseWorldRotation,
                    BaseScale            = bone.BaseScale,
                };
            }

            newSkeleton.Bones = bones.ToList();
            return newSkeleton;
        }
    }
}
