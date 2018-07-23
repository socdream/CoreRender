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
    }
}
