using CoreMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender
{
    public class Camera
    {
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;
        public float[] ViewMatrix { get; set; }
        public float[] ProjectionMatrix { get; set; }

        public float[] Position { get; set; } = new float[] { 0, 0, 10f };
        public float[] Target { get; set; } = new float[] { 0, 0, 0 };

        public void Update(float time, GLWindow4 control)
        {
            ViewMatrix = new float[] { }.LookAtMatrix(Position, Target, new float[] { 0, 1f, 0 });
            ProjectionMatrix = new float[] { }.PerspectiveFieldOfViewMatrix((float)Math.PI / 3f, (float)control.Width / control.Height, NearPlane, FarPlane);
        }
    }
}
