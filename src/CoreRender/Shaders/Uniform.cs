using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Shaders
{
    public class Uniform
    {
        public string Name { get; set; } = "";
        public int Location { get; set; } = 0;
        public object Value { get; set; }
    }
}
