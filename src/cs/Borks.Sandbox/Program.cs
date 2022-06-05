using Borks.Graphics3D;
using Borks.Graphics3D.SEAnim;
using Borks.Graphics3D.SEModel;
using Borks.Graphics3D.SMD;
using Borks.Graphics3D.CoDXAsset.Tokens;
using System.Security.Cryptography;
using System.Text;
using Borks.Graphics3D.Translator;
using System.Diagnostics;
using SharpGLTF;
using System.Net.Http.Headers;
using System.Numerics;
using System.Data;
using Borks.Cryptography.MurMur3;

namespace Borks.Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            var translatorFactory = new Graphics3DTranslatorFactory().WithDefaultTranslators();

            var model = SharpGLTF.Schema2.ModelRoot.Load(@"C:\Visual Studio\Projects\GreyhoundPublic\Greyhound\src\WraithXCOD\x64\Debug\exported_files\modern_warfare_4\xmodels\body_mp_western_zedra_1_1_lod1\body_mp_western_zedra_1_1_lod1_LOD0.gltf");
            var skin = model.LogicalSkins.Count == 0 ? null : model.LogicalSkins[0];
            var result = new Model();
            var skeleton = new Skeleton();
            result.Skeleton = skeleton;

            //// Add all nodes, then sort out children
            //foreach (var b in model.LogicalNodes)
            //{
            //    b.LocalTransform.GetDecomposed();
            //    if(b.Name is null)
            //    {
            //        skeleton.Bones.Add(new($"bone_{b.LogicalIndex}")
            //        {
            //            //BaseLocalRotation = b.LocalTransform.Rotation,
            //            //BaseLocalTranslation = b.LocalTransform.Translation,
            //        });
            //    }
            //    else
            //    {
            //        skeleton.Bones.Add(new(b.Name.ToLower().Replace(".", "_").Replace(":", "_"))
            //        {
            //            //BaseLocalRotation = b.LocalTransform.Rotation,
            //            //BaseLocalTranslation = b.LocalTransform.Translation,
            //        });
            //    }
            //}

            //foreach (var node in model.LogicalNodes)
            //{
            //    foreach (var child in node.VisualChildren)
            //    {
            //        skeleton.Bones[child.LogicalIndex].Parent = skeleton.Bones[node.LogicalIndex];
            //    }
            //}

            //skeleton.GenerateGlobalTransforms();

            //var matrices = skin.GetInverseBindMatricesAccessor().AsMatrix4x4Array();

            //for (int i = 0; i < skin.JointsCount; i++)
            //{
            //    var gltfJoint = skin.GetJoint(i);
            //    var newBone = skeleton.Bones[gltfJoint.Joint.LogicalIndex];
            //    var matrix = matrices[i];
            //    var asMatrix = Matrix4x4.CreateFromQuaternion(newBone.BaseWorldRotation);
            //    asMatrix.Translation = newBone.BaseWorldTranslation;
            //    var final = matrix * asMatrix;

            //    Matrix4x4.Invert(matrix, out var newMatrix);

            //    newBone.BaseWorldTranslation = newMatrix.Translation;
            //    newBone.BaseWorldRotation = Quaternion.CreateFromRotationMatrix(newMatrix);
            //}

            //skeleton.GenerateLocalTransforms();

            foreach (var gltfMaterial in model.LogicalMaterials)
            {
                result.Materials.Add(new(gltfMaterial.Name.ToLower().Replace(".", "_")));
            }

            foreach (var node in model.LogicalNodes)
            {
                if (node.Mesh is SharpGLTF.Schema2.Mesh gltfMesh)
                {
                    var transform = node.LocalTransform.Matrix;

                    Console.WriteLine(transform.Translation);
                    Console.WriteLine(gltfMesh.LogicalIndex);

                    //if(marv is not null)
                    //{
                    //    transform = marv.LocalTransform.Matrix;
                    //}

                    foreach (var primitive in gltfMesh.Primitives)
                    {

                        if (primitive.DrawPrimitiveType != SharpGLTF.Schema2.PrimitiveType.TRIANGLES)
                            throw new NotSupportedException($"Unsupported Gltf Primitive type: {primitive.DrawPrimitiveType}");

                        // We require at least positions and indices
                        var posAccessor = primitive.GetVertexAccessor("POSITION");
                        if (posAccessor is null)
                            continue;

                        var positions = posAccessor.AsVector3Array();
                        var indices = primitive.IndexAccessor.AsIndicesArray();
                        var faceCount = indices.Count / 3;


                        var newMesh = new Mesh(positions.Count, faceCount, 1, 1, 1, MeshAttributeFlags.None);

                        if (newMesh.Materials.Count == 0)
                        {
                            result.Materials.Add(new("default_borkmat"));
                        }

                        newMesh.Materials.Add(result.Materials[primitive.Material == null ? 0 : primitive.Material.LogicalIndex]);

                        foreach (var position in positions)
                        {
                            newMesh.Positions.Add(Vector3.Transform(position, transform));
                        }

                        for (int i = 0; i < faceCount; i++)
                        {
                            newMesh.Faces.Add((
                                (int)indices[i * 3 + 0],
                                (int)indices[i * 3 + 1],
                                (int)indices[i * 3 + 2]));
                        }

                        // Rest of data is optional, depending on if it is provided by the data
                        var normAccessor = primitive.GetVertexAccessor("NORMAL");

                        if (normAccessor is not null)
                        {
                            var normals = normAccessor.AsVector3Array();

                            foreach (var normal in normals)
                            {
                                newMesh.Normals.Add(Vector3.TransformNormal(normal, transform));
                            }
                        }

                        // Try get skinning for marvooner
                        int boneSets = 0;
                        while (primitive.GetVertexAccessor($"JOINTS_{boneSets}") is not null)
                            boneSets++;

                        if (boneSets > 0)
                        {
                            newMesh.Influences.SetCapacity(positions.Count, boneSets * 4);

                            for (int i = 0; i < boneSets; i++)
                            {
                                var jointsAccessor = primitive.GetVertexAccessor($"JOINTS_{i}");
                                var weightsAccessor = primitive.GetVertexAccessor($"WEIGHTS_{i}");

                                if (jointsAccessor is null || weightsAccessor is null)
                                    throw new Exception("Null skinning info.");

                                var joints = jointsAccessor.AsVector4Array();
                                var weights = weightsAccessor.AsVector4Array();

                                for (int w = 0; w < joints.Count; w++)
                                {
                                    var joint = joints[w];
                                    var weight = weights[w];

                                    newMesh.Influences.Add((skin.GetJoint((int)joint.X).Joint.LogicalIndex, weight.X));
                                    newMesh.Influences.Add((skin.GetJoint((int)joint.Y).Joint.LogicalIndex, weight.Y));
                                    newMesh.Influences.Add((skin.GetJoint((int)joint.Z).Joint.LogicalIndex, weight.Z));
                                    newMesh.Influences.Add((skin.GetJoint((int)joint.W).Joint.LogicalIndex, weight.W));
                                }
                            }
                        }

                        result.Meshes.Add(newMesh);
                    }
                }
            }

            translatorFactory.Save("marv.semodel", result);

            var read = translatorFactory.Load<Model>("marv.semodel");
        }
    }
}