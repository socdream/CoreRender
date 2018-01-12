using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRender.Shaders
{
    public class Shader
    {
        public int Program { get; set; }
        public int FragmentShader { get; set; }
        public int VertexShader { get; set; }
        public string FragmentSource { get; set; }
        public string VertexSource { get; set; }
    }
}
