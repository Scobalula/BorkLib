using System.Numerics;
using System.Text;

namespace Borks.Graphics3D.SMD
{
    /// <summary>
    /// A class to translate to and from Valve SMD.
    /// </summary>
    public sealed class SMDTranslator : Graphics3DTranslator
    {
        /// <inheritdoc/>
        public override string Name => "SMDTranslator";

        /// <inheritdoc/>
        public override string[] Extensions { get; } =
        {
            ".smd"
        };

        /// <inheritdoc/>
        public override bool SupportsReading => true;

        /// <inheritdoc/>
        public override bool SupportsWriting => true;

        /// <inheritdoc/>
        public override void Read(Stream stream, string filePath, Graphics3DTranslatorIO output)
        {
            // Mesh lookup table
            using var reader = new StreamReader(stream, null, true, -1, true);
            var smdReader = new SMDReader(reader);

            SMDHelper.Read(smdReader, out var skeleton, out var model, out var animation);

            if (skeleton != null)
                output.Skeletons.Add(skeleton);
            if (model != null)
                output.Models.Add(model);
            if (animation != null)
                output.Animations.Add(animation);
        }

        /// <inheritdoc/>
        public override void Write(Stream stream, string filePath, Graphics3DTranslatorIO input)
        {
            using var writer = new StreamWriter(stream);

            writer.WriteLine("version 1");

            // Get the skeleton, if not found create default skeleton.
            var skeleton = input.TryGetFirstSkeleton(out var potentialSkeleton) ? potentialSkeleton : new()
            {
                Bones = new()
                {
                    new("root_bone")
                }
            };

            // Begin nodes section
            writer.WriteLine("nodes");
            var index = 0;
            foreach (var bone in skeleton.Bones)
            {
                writer.WriteLine($"{index++} \"{bone.Name}\" {skeleton.Bones.IndexOf(bone.Parent!)}");
            }
            writer.WriteLine("end");
            // Begin skeleton section
            writer.WriteLine("skeleton");
            writer.WriteLine("time 0");
            index = 0;
            foreach (var bone in skeleton.Bones)
            {
                SMDHelper.WriteFrame(writer, index++, bone.LocalPosition, bone.LocalRotation);
            }
            // Check if we have animations
            if (input.Animations.Count > 0)
            {
                var animation = input.Animations[0];
                if(animation.SkeletonAnimation != null)
                {
                    var frames = animation.GetAnimationFrameCount();

                    // Start by building a index map against our skeleton
                    var indexMap = new int[animation.SkeletonAnimation.Targets.Count];
                    index = 0;
                    foreach (var target in animation.SkeletonAnimation.Targets)
                    {
                        indexMap[index++] = skeleton.GetBoneIndex(target.BoneName);
                    }

                    // Next we'll need iterate all bones
                    for (float i = 1; i < frames; i += 1)
                    {
                        writer.WriteLine($"time {(int)i}");

                        index = 0;

                        foreach (var target in animation.SkeletonAnimation.Targets)
                        {
                            var targetIndex = indexMap[index];

                            if (targetIndex != -1)
                            {
                                // Sample at this frame
                                var translation = target.SampleTranslation(i);
                                var rotation = target.SampleRotation(i);

                                SMDHelper.WriteFrame(writer, index++, translation, rotation);
                            }
                        }
                    }
                }
            }
            writer.WriteLine("end");
            // Begin Triangles
            if(input.Models.Count > 0)
            {
                var model = input.Models[0];

                writer.WriteLine("triangles");

                foreach (var mesh in model.Meshes)
                {
                    var materialName = mesh.Materials[0].Name;

                    var hasUVs     = mesh.UVLayers.Count == mesh.Positions.Count;
                    var hasNormals = mesh.Normals.Count == mesh.Positions.Count;
                    var hasWeights = mesh.Influences.Count == mesh.Positions.Count;
                    
                    foreach (var (i0, i1, i2) in mesh.Faces)
                    {
                        writer.WriteLine(materialName);

                        // Write 3 vertices for this single face/triangle.
                        SMDHelper.WriteVertex(writer, i0, mesh, hasNormals, hasUVs, hasWeights);
                        SMDHelper.WriteVertex(writer, i1, mesh, hasNormals, hasUVs, hasWeights);
                        SMDHelper.WriteVertex(writer, i2, mesh, hasNormals, hasUVs, hasWeights);
                    }
                }

                writer.WriteLine("end");
            }
        }

        /// <inheritdoc/>
        public override bool IsValid(Span<byte> startOfFile, Stream stream, string? filePath, string? ext)
        {
            return !string.IsNullOrWhiteSpace(ext) && Extensions.Contains(ext);
        }
    }
}