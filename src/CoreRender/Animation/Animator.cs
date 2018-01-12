using CoreCollada;
using CoreFBX.FBX;
using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Animation
{
    public class Animator
    {
        public float Length { get { return Frames / FPS; } }
        public float FPS { get; set; } = 30f;
        public int Frames { get; set; }

        public Dictionary<string, List<AnimationKey>> AnimationKeys { get; set; }
        public Dictionary<string, AnimationKey>PoseKeys { get; set; }

        public Animator(FBXFile file)
        {
            var bones = Bone.GetBoneHierarchy(file);

            var boneCurveNodes = new SortedList<string, List<CoreFBX.FBX.Animation.FBXAnimCurveNode>>();
            AnimationKeys = new Dictionary<string, List<AnimationKey>>();
            PoseKeys = new Dictionary<string, AnimationKey>();
            
            foreach(var bone in bones)
            {
                AnimationKeys.Add(bone.Id, new List<AnimationKey>());
                var curveNodes = FBX.FBXHelper.GetAnimationCurveNodes(file, bone);

                // get curves for the curve nodes of the bone
                foreach(var curveNode in curveNodes)
                {
                    var curves = file.GetAnimationCurves(curveNode);

                    foreach(var curve in curves)
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

                boneCurveNodes.Add(bone.Id, curveNodes);
            }

            // convert curves to animation keys
            var keys = boneCurveNodes.SelectMany(a => a.Value.SelectMany(c => c.Curves.SelectMany(b => b.Value.KeyTime))).Distinct().OrderBy(a => a).ToArray();
            Frames = keys.Length;

            for (int i = 0; i < keys.Length; i++)
            {
                foreach (var bone in bones)
                {
                    var animKey = new AnimationKey()
                    {
                        Frame = i,
                        Transform = bone.JointMatrix
                    };

                    // Translation
                    var curveNode = boneCurveNodes[bone.Id].Where(a => a.Name.Contains("T")).FirstOrDefault();

                    if(curveNode != null)
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
                    curveNode = boneCurveNodes[bone.Id].Where(a => a.Name.Contains("R")).FirstOrDefault();

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
                    curveNode = boneCurveNodes[bone.Id].Where(a => a.Name.Contains("S")).FirstOrDefault();

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
                    bone.Keyframes.Add(animKey);
                    //AnimationKeys[bone.Id].Add(animKey);
                }
            }

            //restore bind pose
            foreach (var bone in bones)
            {
                var animKey = new AnimationKey()
                {
                    Frame = 0,
                    Transform = bone.JointMatrix
                };

                // Translation
                var curveNode = boneCurveNodes[bone.Id].Where(a => a.Name.Contains("T")).FirstOrDefault();

                if (curveNode != null)
                {
                    animKey.Translation = curveNode.Value;
                }

                // Rotation
                curveNode = boneCurveNodes[bone.Id].Where(a => a.Name.Contains("R")).FirstOrDefault();

                if (curveNode != null)
                {
                    animKey.Rotation = curveNode.Value.ToRadians().QuaternionFromEuler(EulerOrder.XYZ);
                }

                // Scale
                curveNode = boneCurveNodes[bone.Id].Where(a => a.Name.Contains("S")).FirstOrDefault();

                if (curveNode != null)
                {
                    animKey.Scale = curveNode.Value;
                }

                // add the animation key to the list
                PoseKeys.Add(bone.Name, animKey);
            }
        }// Constructor()

        public Animator(CoreCollada.Collada file)
        {
            var bones = Bone.GetBoneHierarchy(file);

            var boneCurveNodes = new SortedList<string, List<CoreFBX.FBX.Animation.FBXAnimCurveNode>>();
            AnimationKeys = new Dictionary<string, List<AnimationKey>>();
            PoseKeys = new Dictionary<string, AnimationKey>();

            foreach (var bone in bones)
                AnimationKeys.Add(bone.Id, bone.Keyframes);
        }// Constructor(Collada)

        public void UpdateWorldMatrices(int frame, List<Bone> bones)
        {
            foreach (var bone in bones)
            {
                if (frame == -1)
                {
                    //pose node
                    bone.WorldMatrix = new float[] { }.MatrixCompose(PoseKeys[bone.Id].Translation, PoseKeys[bone.Id].Rotation, PoseKeys[bone.Id].Scale);
                }
                else
                {
                    bone.WorldMatrix = new float[] { }.MatrixCompose(AnimationKeys[bone.Id][frame].Translation, AnimationKeys[bone.Id][frame].Rotation, AnimationKeys[bone.Id][frame].Scale);
                }
            }
            
            var root = bones.Where(a => a.Parent == "").FirstOrDefault();
        }
    }
}
