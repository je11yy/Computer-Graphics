using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace lab3 {
    public class Shader {
        public readonly int Handle;

        public Shader() {}

        public Shader(string vertexPath, string fragmentPath) {
            var VertexShaderSource = File.ReadAllText(vertexPath);

            var FragmentShaderSource = File.ReadAllText(fragmentPath);

            var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            CompileShader(VertexShader);
            
            CompileShader(FragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var success_2);
        }

        public void SetMatrix4(String name, Matrix4 matrix) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(String name, Vector3 vector) {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, vector.X, vector.Y, vector.Z);
        }

        public int GetUniformLocation(string name) {
            return GL.GetUniformLocation(Handle, name);
        }

        private static void CompileShader(int shader) {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True) {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred while compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program) {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True) {
                throw new Exception($"Error occurred while linking Program({program})");
            }
        }

        public void Use() {
            GL.UseProgram(Handle);
        }

        protected virtual void Dispose(bool disposing) {
            GL.DeleteProgram(Handle);
        }

        private void Dispose() {
            Dispose(true);
        }
    }
}