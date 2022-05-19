using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Borks.Graphics3D
{
    /// <summary>
    /// Helper methods for merging <see cref="Model"/>s together.
    /// </summary>
    public static class ModelMerger
    {
        /// <summary>
        /// Checks if the provided model can be connected to any of the models in the provided list.
        /// </summary>
        /// <param name="toConnect">The model to connect.</param>
        /// <param name="models">The models to check, if the model that is to be connected is contained within this, it is skipped.</param>
        /// <param name="ignoreRoots">Ignore root bones the check.</param>
        /// <returns>True if the model can be connected, otherwise false.</returns>
        public static bool CanConnect(Model toConnect, IEnumerable<Model> models, bool ignoreRoots)
        {
            if(toConnect.Skeleton != null)
            {
                var toConnectRoot = toConnect.Skeleton?.Bones.First(x => x.Parent == null);

                foreach (var model in models)
                {
                    if(model != toConnect)
                    {
                        if (ignoreRoots)
                        {
                            var potentialRoot = model.Skeleton?.Bones.First(x => x.Parent == null);

                            if (potentialRoot?.Name.Equals(toConnectRoot?.Name) == true)
                            {
                                continue;
                            }
                        }

                        if (model.Skeleton?.ContainsBone(toConnectRoot?.Name) == true)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to locate the root model by determining the model that cannot be connected to other models.
        /// </summary>
        /// <param name="models">Models to search.</param>
        /// <param name="root">The model found, if none are found, this will be null.</param>
        /// <returns>True if a suitable root model is found, otherwise false.</returns>
        public static bool TryGetRootModel(IEnumerable<Model> models,[NotNullWhen(true)] out Model? root)
        {
            // Models that are not valid (can be connected to other models)
            var invalid = new List<Model>();

            foreach (var model in models)
            {
                // Models without skeletons are not valid for root
                if (model.Skeleton != null)
                {
                    if(!CanConnect(model, models, false))
                    {
                        root = model;
                        return true;
                    }
                }
                else
                {
                    invalid.Add(model);
                }
            }

            // Second pass if the above failed
            foreach (var model in models)
            {
                // Models without skeletons are not valid for root
                if (!invalid.Contains(model) && model.Skeleton != null)
                {
                    if(CanConnect(model, models, true))
                    {
                        invalid.Add(model);
                    }
                }
            }

            root = models.First(x => !invalid.Contains(x));
            return root != null;
        }

        /// <summary>
        /// Merges the provided models with the root model provided.
        /// </summary>
        /// <param name="root">Root model to merge the other models into.</param>
        /// <param name="models">The models to merge with the root model, if the root model is contained within this, it is skipped.</param>
        public static void Merge(Model root, IEnumerable<Model> models)
        {
            var merged = new List<Model>()
            {
                root
            };

            // Keep looping until we've processed all models
            // We need to do this as a model might connect to another
            // that hasn't been connected to root yet.
            while(true)
            {
                bool reloop = false;

                foreach (var model in models)
                {

                    // Check if we've processed, also considers root as it's added to merged
                    if (merged.Contains(model))
                        continue;

                    var translation = Vector3.Zero;
                    var rotation = Quaternion.Identity;

                    // Check for a skeleton first, if we have none, then merge anyway
                    if(model.Skeleton != null && root.Skeleton != null)
                    {
                        // If we have a model that doesn't exist, and can be connected, we must wait 
                        // for it's parent model to be connected
                        var rootBone = model.Skeleton.Bones.Find(x => x.Parent == null)!;

                        if (!root.Skeleton.ContainsBone(rootBone.Name) && CanConnect(model, models, false))
                        {
                            // Mark for reloop and continue to the next
                            reloop = true;
                            continue;
                        }


                        // Copy over bones
                        foreach (var bone in model.Skeleton.Bones)
                        {
                            if(!root.Skeleton.ContainsBone(bone.Name))
                            {
                                var nBone = new SkeletonBone(bone.Name)
                                {
                                    BaseLocalTranslation = bone.BaseLocalTranslation,
                                    BaseLocalRotation = bone.BaseLocalRotation,
                                    Index         = root.Skeleton.Bones.Count,
                                    Parent        = root.Skeleton.GetBone(bone.Parent?.Name)
                                };

                                root.Skeleton.Bones.Add(nBone);
                            }
                        }

                        // Compute global positions (we need them for offsetting)
                        root.Skeleton.GenerateGlobalTransforms();
                        model.Skeleton.GenerateGlobalTransforms();

                        // Get root and the new root, to compute offsets
                        var newRoot = root.Skeleton.GetBone(rootBone.Name);

                        // TODO: compute this for each bone and utilize weights
                        // but as an option, as it may cause severe deformations 
                        // if bones have moved
                        if(newRoot != null)
                        {
                            translation = newRoot.BaseWorldTranslation - rootBone.BaseWorldTranslation;
                            rotation = (newRoot.BaseWorldRotation * Quaternion.Inverse(rootBone.BaseWorldRotation));
                        }
                    }

                    // At this point we are sure we haven't processed it, let's do it
                    merged.Add(model);

                    // Now lets process each mesh
                    foreach (var mesh in model.Meshes)
                    {
                        var newMesh = new Mesh(mesh);
                        var vertexCount = newMesh.Positions.Count;

                        for (int i = 0; i < vertexCount; i++)
                        {
                            newMesh.Positions[i] = Vector3.Transform(newMesh.Positions[i], rotation) + translation;

                            if (newMesh.Normals.ElementCount != 0)
                                newMesh.Normals[i] = Vector3.Normalize(Vector3.Transform(newMesh.Normals[i], rotation));
                            if (newMesh.Tangents.ElementCount != 0)
                                newMesh.Tangents[i] = Vector3.Normalize(Vector3.Transform(newMesh.Tangents[i], rotation));
                            if (newMesh.BiTangents.ElementCount != 0)
                                newMesh.BiTangents[i] = Vector3.Normalize(Vector3.Transform(newMesh.BiTangents[i], rotation));

                            // Remap influences to the new skeleton if we have any
                            if(newMesh.Influences.ElementCount != 0 && root.Skeleton != null && model.Skeleton != null)
                            {
                                for (int v = 0; v < newMesh.Influences.Dimension; v++)
                                {
                                    var (index, weight) = newMesh.Influences[i, v];

                                    // Weights are null terminated by influence
                                    if (weight == 0)
                                        break;

                                    newMesh.Influences[i, v] = (root.Skeleton.GetBoneIndex(model.Skeleton.Bones[index].Name), weight);
                                }
                            }
                            else
                            {
                                newMesh.Influences.Clear();
                            }
                        }

                        // Finally let's remap materials
                        for (int i = 0; i < newMesh.Materials.Count; i++)
                        {
                            // Check our material for existing one
                            // and update this mesh accordingly
                            var curMaterial = newMesh.Materials[i];
                            var newMaterial = root.Materials.Find(x => x.Name.Equals(curMaterial.Name));

                            if (newMaterial == null)
                            {
                                newMaterial = new Material(curMaterial.Name);

                                foreach (var (type, texture) in curMaterial.Textures)
                                {
                                    newMaterial.Textures.Add(type, new(texture.FilePath, type));
                                }

                                root.Materials.Add(newMaterial);
                            }

                            newMesh.Materials[i] = newMaterial;
                        }

                        root.Meshes.Add(newMesh);
                    }
                }

                if (!reloop)
                    break;
            }

            // Ensure our indices assigned to bones are correct
            root.AssignSkeletonBoneIndices();
        }

        public static bool Attach(Model root, Model toConnect, string targetBoneName)
        {
            if (root.Skeleton == null)
                return false;
            if (toConnect.Skeleton == null)
                return false;
            if (!root.Skeleton.TryGetBone(targetBoneName, out var targetBone))
                return false;

            var rootSkeleton = root.Skeleton;
            var currSkeleton = toConnect.Skeleton;

            // Connect to new bone
            var rootBone = currSkeleton.GetFirstRoot();

            if (rootBone == null)
                return false;

            // New parent
            currSkeleton.GenerateGlobalTransforms();

            // Copy over bones
            foreach (var bone in currSkeleton.Bones)
            {
                if (!rootSkeleton.ContainsBone(bone.Name))
                {
                    var nBone = new SkeletonBone(bone.Name)
                    {
                        BaseLocalTranslation = bone.BaseLocalTranslation,
                        BaseLocalRotation = bone.BaseLocalRotation,
                        Index = rootSkeleton.Bones.Count,
                        Parent = bone.Parent == null ? targetBone : rootSkeleton.GetBone(bone.Parent?.Name),
                        CanAnimate = bone.CanAnimate
                    };

                    rootSkeleton.Bones.Add(nBone);
                }
            }

            // Compute global positions (we need them for offsetting)
            rootSkeleton.GenerateGlobalTransforms();
            currSkeleton.GenerateGlobalTransforms();

            // Get root and the new root, to compute offsets
            var newRoot = root.Skeleton.GetBone(rootBone.Name)!;

            var translation = newRoot.BaseWorldTranslation - rootBone.BaseWorldTranslation;
            var rotation = (newRoot.BaseWorldRotation * Quaternion.Inverse(rootBone.BaseWorldRotation));

            // Now lets process each mesh
            foreach (var mesh in toConnect.Meshes)
            {
                var newMesh = new Mesh(mesh);
                var vertexCount = newMesh.Positions.Count;

                for (int i = 0; i < vertexCount; i++)
                {
                    newMesh.Positions[i] = Vector3.Transform(newMesh.Positions[i], rotation) + translation;

                    if (newMesh.Normals.ElementCount != 0)
                        newMesh.Normals[i] = Vector3.Normalize(Vector3.Transform(newMesh.Normals[i], rotation));
                    if (newMesh.Tangents.ElementCount != 0)
                        newMesh.Tangents[i] = Vector3.Normalize(Vector3.Transform(newMesh.Tangents[i], rotation));
                    if (newMesh.BiTangents.ElementCount != 0)
                        newMesh.BiTangents[i] = Vector3.Normalize(Vector3.Transform(newMesh.BiTangents[i], rotation));

                    // Remap influences to the new skeleton if we have any
                    if (newMesh.Influences.ElementCount != 0)
                    {
                        for (int v = 0; v < newMesh.Influences.Dimension; v++)
                        {
                            var (index, weight) = newMesh.Influences[i, v];

                            // Weights are null terminated by influence
                            if (weight == 0)
                                break;

                            newMesh.Influences[i, v] = (rootSkeleton.GetBoneIndex(currSkeleton.Bones[index].Name), weight);
                        }
                    }
                    else
                    {
                        newMesh.Influences.Clear();
                    }
                }

                // Finally let's remap materials
                for (int i = 0; i < newMesh.Materials.Count; i++)
                {
                    // Check our material for existing one
                    // and update this mesh accordingly
                    var curMaterial = newMesh.Materials[i];
                    var newMaterial = root.Materials.Find(x => x.Name.Equals(curMaterial.Name));

                    if (newMaterial == null)
                    {
                        newMaterial = new Material(curMaterial.Name);

                        foreach (var (type, texture) in curMaterial.Textures)
                        {
                            newMaterial.Textures.Add(type, new(texture.FilePath, type));
                        }

                        root.Materials.Add(newMaterial);
                    }

                    newMesh.Materials[i] = newMaterial;
                }

                root.Meshes.Add(newMesh);
            }

            return true;
        }
    }
}
