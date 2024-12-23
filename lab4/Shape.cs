using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace lab4 {
    public class Shape(Shader shader, Camera camera)
    {
        private float[] vertices = [];
        private uint[] indices = [];
        private readonly Shader shader = shader;
        private int VBO, VAO;
        private int indicesVBO;
        private readonly Camera camera = camera;
        private Vector3 lightColor = new(Color4.White.R, Color4.White.G, Color4.White.B);
        private Vector3 color = new(Color4.DarkSalmon.R, Color4.DarkSalmon.G, Color4.DarkSalmon.B);
        private float lightingStrength = 1f;

        public void Initialize(float[] vertices, uint[] indices) {
            this.vertices = vertices;
            this.indices = indices;
        }

        public void Load() {
            shader.Use();
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            indicesVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(shader.GetAttribLocation("aPos"));
            GL.VertexAttribPointer(shader.GetAttribLocation("aPos"), 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(shader.GetAttribLocation("aNormal"));
            GL.VertexAttribPointer(shader.GetAttribLocation("aNormal"), 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            LoadPerspective();
            LoadView();
            LoadModel();

            UseShader();
        }

        private void LoadPerspective() {
            if (shader != null) {
                Matrix4 projection = camera.GetProjectionMatrix();
                int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");
                GL.UniformMatrix4(projectionLocation, false, ref projection);
            }
        }

        private void LoadView() {
            if (shader != null) {
                Matrix4 view = camera.GetViewMatrix();
                int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
                GL.UniformMatrix4(viewLocation, false, ref view);
            }
        }

        private void LoadModel() {
            if (shader != null) {
                Matrix4 model = Matrix4.Identity;
                int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
                GL.UniformMatrix4(modelLocation, false, ref model);
            }
        }

        public void UseShader() {
            shader.Use();

            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "inputColor"), ref color);

            Vector3 position = camera.Position;
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "light.position"), ref position);

            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "light.color"), ref lightColor);
            
            Vector3 front = camera.Front;
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "light.direction"), ref front);

            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "light.cutOff"), MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "light.outerCutOff"), MathF.Cos(MathHelper.DegreesToRadians(17.5f)));
            
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "light.constant"), lightingStrength);

            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "light.linear"), 0.09f);

            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "light.quadratic"), 0.032f);

            Vector3 ambient = new(0.2f);
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "light.ambient"), ref ambient);

            Vector3 diffuse = new(0.5f);
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "light.diffuse"), ref diffuse);
        }

        public void DecreaseLightingStrength() {
            lightingStrength += 0.01f;
            UseShader();
        }

        public void IncreaseLightingStrength() {
            lightingStrength -= 0.01f;
            UseShader();
        }

        public void Render() {
            // if (texture != null) {
            //     shader.Use();
            //     GL.BindVertexArray(VAO);
            //     // texture?.Use(TextureUnit.Texture0);
            //     GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            // }

            UseShader();
            LoadPerspective();
            LoadView();
            LoadModel();

            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void Unload() {
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(indicesVBO);
            GL.DeleteVertexArray(VAO);
        }

        protected void UpdateVertexBuffer() {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
        }
    }
}