using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Animation
{
    public class AnimationKey
    {
        public int Frame { get; set; }
        public float[] Translation { get; set; } = new float[] { 0, 0, 0 };
        public float[] Rotation { get; set; } = new float[] { 0, 0, 0, 1 };
        public float[] Scale { get; set; } = new float[] { 1, 1, 1 };
        public float[] Transform
        {
            get
            {
                //return (new float[] { }).MatrixCompose(Translation, Rotation, Scale);
                return (new float[] { }).MatrixCompose(Translation, Rotation, Scale);
            }
            set
            {
                float[] r, t, s;

                value.MatrixDecompose(out t, out r, out s);

                Translation = t;
                Rotation = r;
                Scale = s;
            }
        }
    }
}
