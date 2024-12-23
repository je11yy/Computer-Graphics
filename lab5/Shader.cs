using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text;

namespace RayTracing {
    public class Shader {
        public readonly int Handle;
        public Shader(string vertPath, string fragPath) {
            var shaderSource = File.ReadAllText(vertPath, Encoding.UTF8);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertexShader, shaderSource);

            CompileShader(vertexShader);

            shaderSource = File.ReadAllText(fragPath, Encoding.UTF8);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
        }

        private static void CompileShader(int shader) {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True) {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program) {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True) {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use() {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName) {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
        }

        public void SetFloat(string name, float data) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
        }

        public void SetMatrix4(string name, Matrix4 data) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref data);
        }

        public void SetVector3(string name, Vector3 data) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, data);
        }

        public void SetVector2(string name, Vector2 data) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform2(location, data);
        }
    }
}