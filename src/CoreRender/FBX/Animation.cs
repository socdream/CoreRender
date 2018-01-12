using CoreFBX.FBX;
using CoreFBX.FBX.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.FBX
{
    public class Animation
    {
        public Dictionary<long, Dictionary<string, FBXAnimCurveNode>> Curves { get; set; } = new Dictionary<long, Dictionary<string, FBXAnimCurveNode>>();
        public float Length { get; set; }
        public float FPS { get; set; } = 30f;
        public float Frames { get; set; }

        public Animation(FBXFile file, List<string> bones)
        {
            var rawNodes = file.GetAnimationCurveNodes();
            var rawCurves = file.GetAnimationCurves();

            // first: expand AnimationCurveNode into curve nodes
            var curveNodes = new List<FBXAnimCurveNode>();

            foreach (var tempNode in rawNodes)
            {
                var fileNode = file.FindChild(tempNode.Id);

                curveNodes.Add(new FBXAnimCurveNode(fileNode, file, bones));
            }

            // second: gen dict, mapped by internalId
            var tmp = new Dictionary<long, FBXAnimCurveNode>();

            for (var i = 0; i < curveNodes.Count; ++i)
            {
                tmp.Add(curveNodes[i].Id, curveNodes[i]);
            }

            // third: insert curves into the dict 
            var ac = new List<FBXAnimCurve>();
            var max = 0f;

            foreach (var curve in rawCurves)
            {
                ac.Add(curve);

                max = curve.Length > max ? curve.Length : max;

                var parentId = file.Connections.Where(a => a.Src == curve.Id).FirstOrDefault().Dst;
                var axis = file.GetConnectionType(curve.Id, parentId);

                if (axis.Contains("X"))
                    axis = "x";
                if (axis.Contains("Y"))
                    axis = "y";
                if (axis.Contains("Z"))
                    axis = "z";

                tmp[parentId].Curves.Add(axis, curve);
            }

            // forth: 
            foreach (var t in tmp)
            {
                var id = t.Value.ContainerBoneId;

                if (!Curves.ContainsKey(id))
                {
                    Curves.Add(id, new Dictionary<string, FBXAnimCurveNode>());
                }

                if (Curves[id].ContainsKey(t.Value.Attr))
                    Curves[id][t.Value.Attr] = t.Value;
                else
                    Curves[id].Add(t.Value.Attr, t.Value);
            }

            Length = max;
            Frames = Length * FPS;
        }
    }
}
