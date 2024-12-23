using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace lab3 {
    public class Background(string path) {
        private readonly string path = path;
        private Shader? shader;
        private Texture? texture;
        public Shape? shape;
        private int VAO, VBO;
        private Matrix4 model, view, projection;
        private readonly float[] coordinates = [
            // Front face
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            // Back face
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 

            // Left face
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,

            // Right face
            0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 
            0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  0.0f, 0.0f,

            // Bottom face
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            // Top face
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f, 0.0f, 0.0f,
        ];

        private void LoadVBO() {
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, coordinates.Length * sizeof(float), coordinates, BufferUsageHint.DynamicDraw);
        }

        private void LoadVAO() {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
        }

        private void LoadPerspective(int width, int height) {
            if (shader != null) {
                projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / (float)height, 0.1f, 100f);
                // projection = Matrix4.Identity;
                int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");
                GL.UniformMatrix4(projectionLocation, false, ref projection);
            }
        }

        private void LoadView() {
            if (shader != null && shape != null) {
                Vector3 targetPosition = shape.GetPosition();
                targetPosition.Z = 0f;
                Vector3 cameraPosition = new(0.0f, 0f, 1f);
                view = Matrix4.LookAt(cameraPosition, targetPosition - cameraPosition, Vector3.UnitY);
                int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
                GL.UniformMatrix4(viewLocation, false, ref view);
            }
        }

        private void LoadModel() {
            if (shader != null) {
                model = Matrix4.CreateTranslation(0f, 0.0f, 2.5f);
                int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
                GL.UniformMatrix4(modelLocation, false, ref model);
            }
        }

        public void Load(int width, int height) {
            LoadVBO();
            LoadVAO();

            shader = new Shader("shaders/bgShader.vert", "shaders/bgShader.frag");
            shader.Use();

            LoadView();
            LoadModel();
            LoadPerspective(width, height);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            texture = Texture.LoadFromFile(path);
            texture.Use(TextureUnit.Texture0);
        }

        public void Draw() {
            if (shader != null) {
                shader.Use();
                LoadView();
                texture?.Use(TextureUnit.Texture0);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }

        public void Unload() {
            if (shader != null) {
                GL.DeleteBuffer(VBO);
                GL.DeleteVertexArray(VAO);
                GL.DeleteProgram(shader.Handle);
            }
        }
    }
}