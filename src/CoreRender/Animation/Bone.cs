using CoreCollada;
using CoreFBX.FBX;
using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Animation
{
    public class Bone
    {
        public string Id { get; set; }
        public string Parent { get; set; }
        public List<string> Children { get; set; } = new List<string>();
        public string Name { get; set; }

        /// <summary>
        /// All Key frames for this Bone’s animation
        /// </summary>
        public List<AnimationKey> Keyframes { get; set; } = new List<AnimationKey>();
        
        /// <summary>
        /// Bind pose matrix
        /// </summary>
        public float[] JointMatrix { get; set; } = new float[] { }.IdentityMatrix();

        /// <summary>
        /// The Inverse Bind Pose Matrix
        /// The inverse of the joint’s bind-space transformation matrix at the time the
        /// bind shape was bound to this joint.
        /// Applying this matrix converts vertices from model space to joint space
        /// </summary>
        public float[] InverseBindMatrix { get; set; } = new float[] { }.IdentityMatrix();

        /// <summary>
        /// The World Matrix
        /// </summary>
        public float[] WorldMatrix { get; set; }

        public Bone() { }

        public static List<Bone> GetBoneHierarchy(FBXTreeNode node, FBXFile fbx)
        {
            var bones = new List<Bone>();
            var modelNodes = node.Children.Where(a => a.Node.Name == "Model");

            foreach (var child in modelNodes)
            {
                var model = new Model(child.Node, fbx);

                if (model.ModelType == Model.FBXModelType.LimbNode || (model.ModelType == Model.FBXModelType.None && !model.HasGeometry))
                {
                    var children = GetBoneHierarchy(child, fbx);

                    var t = model.LclTranslation;
                    var r = model.LclRotation.ToRadians().QuaternionFromEuler(EulerOrder.XYZ);
                    var s = model.LclScaling;

                    var deformerId = fbx.Connections.Where(a => a.Src == model.Id && fbx.FindChild(a.Dst).Name == "Deformer").FirstOrDefault();
                    var globalInverseMatrix = new float[] { }.IdentityMatrix();

                    if(deformerId != null)
                    {
                        var deformer = new Deformer(fbx.FindChild(deformerId.Dst));
                    }

                    var bone = new Bone()
                    {
                        Id = model.Id.ToString(),
                        Parent = (node.Node != null) ? node.Node.Id.ToString() : "",
                        Children = children.Select(a => a.Id).ToList(),
                        JointMatrix = new float[] { }.MatrixCompose(t, r, s),
                        Name = model.Name
                    };

                    bone.Keyframes = GetBoneKeyFrames(bone, fbx);

                    bones.Add(bone);
                    
                    bones.AddRange(children);
                }
            }

            return bones;
        }

        public static List<Bone> GetBoneHierarchy(FBXFile file, FBXTreeNode current = null)
        {
            var bones = new List<Bone>();

            if (current == null)
                current = file.RootNode;

            var children = current.Children
                .Where(a => a.Node.Name == "Model")
                .Select(a => new Model(a.Node, file))
                .Where(a => a.ModelType == Model.FBXModelType.LimbNode || (a.ModelType == Model.FBXModelType.None && !a.HasGeometry));

            foreach (var node in children)
            {
                var t = node.LclTranslation;
                var r = node.LclRotation.ToRadians().QuaternionFromEuler(EulerOrder.XYZ);
                var s = node.LclScaling;
                
                var bone = new Bone()
                {
                    Parent = current.Node == null ? "" : current.Node.Id.ToString(),
                    JointMatrix = new float[] { }.MatrixCompose(t, r, s),
                    Id = node.Id.ToString(),
                    Name = node.Name
                };

                bone.Keyframes = GetBoneKeyFrames(bone, file);

                var childrenHier = GetBoneHierarchy(file, current.Children.Where(a => a.Node.Id == node.Id).FirstOrDefault());

                bone.Children = childrenHier.Where(a => a.Parent == bone.Id).Select(a => a.Id).ToList();

                bones.Add(bone);

                bones.AddRange(childrenHier);
            }

            return bones;
        }

        public static List<Bone> GetBoneHierarchy(CoreCollada.Collada file, Node current = null)
        {
            var bones = new List<Bone>();

            var children = (current == null) ?
                file.LibraryVisualScenes[0].Nodes.Where(a => a.Matrix != null).Take(1)
                : current.Nodes;
            
            foreach(var node in children)
            {
                var bone = new Bone()
                {
                    Parent = (current == null) ? "" : current.SId,
                    JointMatrix = node.MatrixValue.TransposeMatrix(),
                    InverseBindMatrix = node.MatrixValue.TransposeMatrix().MatrixInverse(),
                    Id = node.SId,
                    Name = node.Name,
                    Children = node.Nodes.Select(a => a.SId).ToList()
                };

                bone.Keyframes = GetBoneKeyFrames(bone, file);
                
                bones.Add(bone);
                
                bones.AddRange(GetBoneHierarchy(file, node));
            }

            return bones;
        }

        public static List<AnimationKey> GetBoneKeyFrames(Bone bone, FBXFile file)
        {
            var result = new List<AnimationKey>();

            var curveNodes = FBX.FBXHelper.GetAnimationCurveNodes(file, bone);

            // get curves for the curve nodes of the bone
            foreach (var curveNode in curveNodes)
            {
                var curves = file.GetAnimationCurves(curveNode);

                foreach (var curve in curves)
                {
                    // the connection defines the axis being affected
                    var axis = file.GetConnectionType(curve.Id, curveNode.Id);

                    if (axis.Contains("X"))
                        axis = "x";
                    if (axis.Contains("Y"))
                        axis = "y";
                    if (axis.Contains("Z"))
                        axis = "z";

                    curveNode.Curves.Add(axis, curve);
                }
            }

            // convert curves to animation keys
            var keys = curveNodes.SelectMany(c => c.Curves.SelectMany(b => b.Value.KeyTime)).Distinct().OrderBy(a => a).ToArray();
            int frames = keys.Length;

            for (int i = 0; i < keys.Length; i++)
            {
                var animKey = new AnimationKey()
                {
                    Frame = i,
                    Transform = bone.JointMatrix
                };

                // Translation
                var curveNode = curveNodes.Where(a => a.Name.Contains("T")).FirstOrDefault();

                if (curveNode != null)
                {
                    // check if the curve has a value for that frame first
                    var framePos = curveNode.Curves["x"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var x = (framePos > -1) ? curveNode.Curves["x"].KeyValueFloat[framePos] : animKey.Translation[0];

                    framePos = curveNode.Curves["y"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var y = (framePos > -1) ? curveNode.Curves["y"].KeyValueFloat[framePos] : animKey.Translation[1];

                    framePos = curveNode.Curves["z"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var z = (framePos > -1) ? curveNode.Curves["z"].KeyValueFloat[framePos] : animKey.Translation[2];

                    animKey.Translation = new float[] { x, y, z };
                }

                // Rotation
                curveNode = curveNodes.Where(a => a.Name.Contains("R")).FirstOrDefault();

                if (curveNode != null)
                {
                    // check if the curve has a value for that frame first
                    var framePos = curveNode.Curves["x"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var x = (framePos > -1) ? curveNode.Curves["x"].KeyValueFloat[framePos] : 0F;

                    framePos = curveNode.Curves["y"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var y = (framePos > -1) ? curveNode.Curves["y"].KeyValueFloat[framePos] : 0f;

                    framePos = curveNode.Curves["z"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var z = (framePos > -1) ? curveNode.Curves["z"].KeyValueFloat[framePos] : 0f;

                    animKey.Rotation = new float[] { x.ToRadians(), y.ToRadians(), z.ToRadians() }.QuaternionFromEuler(EulerOrder.XYZ);
                }

                // Scale
                curveNode = curveNodes.Where(a => a.Name.Contains("S")).FirstOrDefault();

                if (curveNode != null)
                {
                    // check if the curve has a value for that frame first
                    var framePos = curveNode.Curves["x"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var x = (framePos > -1) ? curveNode.Curves["x"].KeyValueFloat[framePos] : 0f;

                    framePos = curveNode.Curves["y"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var y = (framePos > -1) ? curveNode.Curves["y"].KeyValueFloat[framePos] : 0f;

                    framePos = curveNode.Curves["z"].KeyTime.TakeWhile(a => a < keys[i]).Count() - 1;
                    var z = (framePos > -1) ? curveNode.Curves["z"].KeyValueFloat[framePos] : 0f;

                    animKey.Scale = new float[] { x, y, z };
                }

                // add the animation key to the list
                result.Add(animKey);
            }

            return result;
        }

        public static List<AnimationKey> GetBoneKeyFrames(Bone bone, CoreCollada.Collada file)
        {
            var result = new List<AnimationKey>();

            var animation = file.LibraryAnimations.Where(a => a.Name == bone.Id).FirstOrDefault();

            if (animation != null)
            {
                var channel = (animation.Animations.Count > 0 && animation.Animations[0].Channels.Count > 0) ?
                    animation.Animations[0].Channels[0]
                    : null;

                if (channel != null)
                {
                    var inputName = animation.Animations[0].Samplers[0].Inputs[0].Source;
                    var outputName = animation.Animations[0].Samplers[0].Inputs[1].Source;
                    var interpolationName = animation.Animations[0].Samplers[0].Inputs[2].Source;

                    // time array
                    var sourceInput = animation.Animations[0].Sources.Where(a => a.Id == inputName.Substring(1)).FirstOrDefault();
                    // actual values
                    var sourceOutput = animation.Animations[0].Sources.Where(a => a.Id == outputName.Substring(1)).FirstOrDefault();
                    var sourceInterpolation = animation.Animations[0].Sources.Where(a => a.Id == interpolationName.Substring(1)).FirstOrDefault();

                    for (var i = 0; i < sourceInput.FloatValues.Length; i++)
                        result.Add(new AnimationKey()
                        {
                            Frame = (int)Math.Round(sourceInput.FloatValues[i] * 30),
                            Transform = sourceOutput.FloatValues.Skip(i * 16).Take(16).ToArray().TransposeMatrix()
                        });
                }
            }

            return result;
        }
    }
}
