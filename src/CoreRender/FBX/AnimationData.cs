using CoreFBX.FBX.Animation;
using CoreMath;
using CoreRender.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.FBX
{
    public class AnimationData
    {
        public string Name { get; set; }
        public float FPS { get; set; } = 30f;
        public float Length { get; set; }
        public List<AnimationDataItem> Hierarchy { get; set; } = new List<AnimationDataItem>();

        public AnimationData(List<Bone> bones, Animation animations)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                var name = bones[i].Id;

                Hierarchy.Add(new AnimationDataItem() { Parent = bones[i].Parent, Name = name, Keys = new List<Key>() });
            }

            for (int frame = 0; frame < animations.Frames; frame++)
            {
                for (int i = 0; i < bones.Count; i++)
                {
                    var bone = bones[i];
                    var animNode = animations.Curves[i];
                    
                    for (int j = 0; j < Hierarchy.Count; j++)
                    {
                        if(Hierarchy[j].Name == bone.Id)
                        {
                            Hierarchy[j].Keys.Add(GenKey(animNode, bone, frame));
                        }
                    }
                }
            }
        }

        public bool HasCurve(Dictionary<string, FBXAnimCurveNode> animNode, string attr)
        {
            FBXAnimCurveNode attrNode = null;

            switch (attr)
            {
                case "S":
                    if (!animNode.ContainsKey("S"))
                        return false;
                    attrNode = animNode["S"];
                    break;
                case "R":
                    if (!animNode.ContainsKey("R"))
                        return false;
                    attrNode = animNode["R"];
                    break;
                case "T":
                    if (!animNode.ContainsKey("T"))
                        return false;
                    attrNode = animNode["T"];
                    break;
            }

            if (!attrNode.Curves.ContainsKey("x"))
                return false;

            if (!attrNode.Curves.ContainsKey("y"))
                return false;

            if (!attrNode.Curves.ContainsKey("z"))
                return false;

            return true;
        }

        public bool HasKeyOnFrame(FBXAnimCurveNode attrNode, int frame)
        {
            var x = IsKeyExistOnFrame(attrNode.Curves["x"], frame);
            var y = IsKeyExistOnFrame(attrNode.Curves["y"], frame);
            var z = IsKeyExistOnFrame(attrNode.Curves["z"], frame);

            return x && y && z;
        }

        public bool IsKeyExistOnFrame(FBXAnimCurve curve, int frame)
        {
            return curve.KeyValueFloat.Length > frame;
        }

        public Key GenKey(Dictionary<string, FBXAnimCurveNode> animNode, Bone bone, int frame)
        {
            // key initialize with its bone's bind pose at first
            float[] t, r, s;

            bone.JointMatrix.MatrixDecompose(out t, out r, out s);

            var key = new Key
            {
                Time = frame / FPS,
                Pos = t,
                Rot = r,
                Scl = s,
            };

            if (animNode == null)
                return key;

            try
            {
                if (HasCurve(animNode, "T") && HasKeyOnFrame(animNode["T"], frame))
                {
                    var pos = new float[] {
                        animNode["T"].Curves["x"].KeyValueFloat[frame],
                        animNode["T"].Curves["y"].KeyValueFloat[frame],
                        animNode["T"].Curves["z"].KeyValueFloat[frame]
                    };

                    key.Pos = pos;
                }
                else
                {
                     //delete key.pos
                }

                if (HasCurve(animNode, "R") && HasKeyOnFrame(animNode["R"], frame))
                {
                    var rot = new float[] {
                        animNode["R"].Curves["x"].KeyValueFloat[frame].ToRadians(),
                        animNode["R"].Curves["y"].KeyValueFloat[frame].ToRadians(),
                        animNode["R"].Curves["z"].KeyValueFloat[frame].ToRadians()
                    }.QuaternionFromEuler(EulerOrder.XYZ);

                    key.Rot = rot;
                }
                else
                {
                    //delete key.rot
                }

                if (HasCurve(animNode, "S") && HasKeyOnFrame(animNode["S"], frame))
                {
                    var scl = new float[] {
                        animNode["S"].Curves["x"].KeyValueFloat[frame],
                        animNode["S"].Curves["y"].KeyValueFloat[frame],
                        animNode["S"].Curves["z"].KeyValueFloat[frame]
                    };

                    key.Pos = scl;
                }
                else
                {
                    //delete key.scl
                }
            }
            catch (Exception)
            {
                // curve is not full plotted
            }

            return key;
        }

        public class AnimationDataItem
        {
            public string Parent { get; set; }
            public string Name { get; set; }
            public List<Key> Keys { get; set; }
        }

        public class Key
        {
            public float Time { get; set; }
            public float[] Pos { get; set; }
            public float[] Rot { get; set; }
            public float[] Scl { get; set; }
        }
    }
}
