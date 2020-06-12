using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Extensions
{
    public static class ColorExtensions
    {
        public static float[] ToFloatArray(this System.Drawing.Color color)
        {
            return new float[]
            {
                color.R/255f,
                color.G/255f,
                color.B/255f,
                color.A/255f
            };
        }

        public static System.Drawing.Color ToColor(this float[] value) => System.Drawing.Color.FromArgb((int)(255f * value[3]), (int)(255f * value[0]), (int)(255f * value[1]), (int)(255f * value[2]));
    }
}
