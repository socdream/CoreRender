using CoreFBX.FBX;
using CoreFBX.FBX.Animation;
using CoreRender.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.FBX
{
    public class FBXHelper
    {

        public static List<FBXAnimCurveNode> GetAnimationCurveNodes(FBXFile file, Bone bone)
        {
            var result = new List<FBXAnimCurveNode>();

            var root = file.RootNode.FindChild(long.Parse(bone.Id));

            var nodes = root.Children;

            foreach (var node in nodes)
            {
                if (node.Node.Name == "AnimationCurveNode")
                    result.Add(new FBXAnimCurveNode(node.Node));

                result.AddRange(file.GetAnimationCurveNodes(node.Node));
            }

            return result;
        }

        public Animation GetAnimations(FBXFile file)
        {
            return new Animation(file, Bone.GetBoneHierarchy(file).Select(a => a.Id).ToList());
        }

        public AnimationData GetAnimationData(FBXFile file)
        {
            return new AnimationData(Bone.GetBoneHierarchy(file), GetAnimations(file));
        }
    }
}
