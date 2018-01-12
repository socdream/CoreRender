using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CoreRender.Shaders
{
    public class ShaderManager
    {
        private static List<Shader> _shaderCache = new List<Shader>();

        private static int CreateShader(string fsShader, ShaderType foType)
        {
            int id = GL.CreateShader(foType);

            GL.ShaderSource(id, fsShader);
            GL.CompileShader(id);
            
            GL.GetShaderInfoLog(id, out string error);

            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);

            return id;
        }

        public static int CreateProgram(int fiVertexShader, int fiFragmentShader)
        {
            int program = GL.CreateProgram();
            GL.AttachShader(program, fiVertexShader);
            GL.AttachShader(program, fiFragmentShader);
            GL.LinkProgram(program);
            GL.UseProgram(program);

            GL.GetProgramInfoLog(program, out string programInfoLog);

            //La variable programInfoLog contiene los errores producidos en el shader en caso de haberlos al compilar
            if (!string.IsNullOrEmpty(programInfoLog))
                throw new Exception(programInfoLog);

            return program;
        }

        public static Shader LoadShader(Shader shader)
        {
            var cached = _shaderCache.Where(a => a.GetType() == shader.GetType()).FirstOrDefault();

            if (cached != null)
                return cached;

            shader.VertexShader = CreateShader(shader.VertexSource, ShaderType.VertexShader);
            shader.FragmentShader = CreateShader(shader.FragmentSource, ShaderType.FragmentShader);
            shader.Program = CreateProgram(shader.VertexShader, shader.FragmentShader);

            if (shader.GetType() !=  typeof(Shader))
                _shaderCache.Add(shader);

            return shader;
        }

        public static void SetUniform(Shader shader, Uniform uniform)
        {
            GL.UseProgram(shader.Program);

            if (uniform.Location == 0)
                uniform.Location = GL.GetUniformLocation(shader.Program, uniform.Name);

            if (uniform.Value is float)
                GL.Uniform1(uniform.Location, (float)uniform.Value);
            else if (uniform.Value is int)
                GL.Uniform1(uniform.Location, (int)uniform.Value);
            else if (uniform.Value is uint)
                GL.Uniform1(uniform.Location, (uint)uniform.Value);
            else if (uniform.Value is short)
                GL.Uniform1(uniform.Location, (short)uniform.Value);
            else if (uniform.Value is ushort)
                GL.Uniform1(uniform.Location, (ushort)uniform.Value);
            else if (uniform.Value is long)
                GL.Uniform1(uniform.Location, (long)uniform.Value);
            else if (uniform.Value is ulong)
                GL.Uniform1(uniform.Location, (ulong)uniform.Value);
            else if (uniform.Value is double)
                GL.Uniform1(uniform.Location, (double)uniform.Value);
            else if (uniform.Value is float[])
                switch (((float[])uniform.Value).Length)
                {
                    case 1:
                        GL.Uniform1(uniform.Location, 1, ((float[])uniform.Value));
                        break;
                    case 2:
                        GL.Uniform2(uniform.Location, 1, ((float[])uniform.Value));
                        break;
                    case 3:
                        GL.Uniform3(uniform.Location, 1, ((float[])uniform.Value));
                        break;
                    case 4:
                        GL.Uniform3(uniform.Location, 1, ((float[])uniform.Value));
                        break;
                    default:
                        break;
                }
            else
                throw new NotSupportedException("Datatype not supported.");
        }
    }
}
