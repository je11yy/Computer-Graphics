using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace lab3 {
    public class Shape {
        private float[] vertices = [];
        private Shader? shader;
        private Texture? texture;
        private int shapeVBO, shapeVAO;
        private Matrix4 shapeModel, shapeView, shapeProjection;
        private bool isAnimated = false;
        private bool elipseAnimationOn = false;
        private Vector3 lightPosition = new(0f, 1.0f, 0f);
        private Vector4 lightColor = new(Color4.White.R, Color4.White.G, Color4.White.B, 1f);
        private float speed = 1f;
        private float animationTime;
        public Shape() {}

        public Shape(float[] vertices) {
            this.vertices = vertices;
        }

        public void Initialize(float[] vertices) {
            this.vertices = vertices;
        }

        private void LoadShapeVBO() {
            shapeVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, shapeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
        }

        private void LoadShapeVAO() {
            shapeVAO = GL.GenVertexArray();
            GL.BindVertexArray(shapeVAO);
        }

        private void LoadPerspective(int width, int height) {
            if (shader != null) {
                shapeProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), width / (float)height, 1f, 100f);
                int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");
                GL.UniformMatrix4(projectionLocation, false, ref shapeProjection);
            }
        }

        private void LoadView() {
            if (shader != null) {
                Vector3 targetPosition = GetPosition();
                targetPosition.Z = 0f;
                Vector3 cameraPosition = new(0.0f, 0f, 3f);
                shapeView = Matrix4.LookAt(cameraPosition, targetPosition - cameraPosition, Vector3.UnitY);
                int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
                GL.UniformMatrix4(viewLocation, false, ref shapeView);
            }
        }

        private void LoadModel() {
            if (shader != null) {
                shapeModel = Matrix4.Identity;
                int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
                GL.UniformMatrix4(modelLocation, false, ref shapeModel);
            }
        }

        public void Load(int width, int height) {
            LoadShapeVBO();
            LoadShapeVAO();

            shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
            shader.Use();

            LoadView();
            LoadModel();
            LoadPerspective(width, height);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "lightColor"), lightColor.X, lightColor.Y, lightColor.Z);
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "lightPos"), lightPosition.X, lightPosition.Y, lightPosition.Z);

            texture = Texture.LoadFromFile("resources/container.png");
            texture.Use(TextureUnit.Texture0);

            Console.WriteLine(GL.GetError());
        }

        public virtual void Move(System.Numerics.Vector3 translation) {
            for (int i = 0; i < vertices.Length; i += 8) {
                vertices[i] += translation.X;
                vertices[i + 1] += translation.Y;
                vertices[i + 2] += translation.Z;
            }
            UpdateVertexBuffer();
        }

        public Vector3 GetPosition() {
            return new Vector3(shapeModel.M41, shapeModel.M42, shapeModel.M43);
        }

        public void Draw(double time) {
            Animate(time);
            if (shader != null) {
                shader.Use();
                LoadView();

                GL.BindVertexArray(shapeVAO);
                texture?.Use(TextureUnit.Texture0);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }

        public void Update(int width, int height) {
            GL.Viewport(0, 0, width, height);
            if (shader != null) {
                GL.UniformMatrix4( GL.GetUniformLocation(shader.Handle, "projection"), false, ref shapeProjection);
            }
        }

        public void ChangeAnimationType() {
            if (elipseAnimationOn) {
                elipseAnimationOn = false;
            } else {
                elipseAnimationOn = true;
            }
        }

        public void ChangeAnimationStatus() {
            if (isAnimated) {
                isAnimated = false;
            } else {
                isAnimated = true;
            }
        }

        public void IncreaseSpeed() {
            speed += 0.1f;
        }

        public void DecreaseSpeed() {
            if (speed > 0f) {
                speed -= 0.1f;
            }
        }

        private void Animate(double time) {
            if (isAnimated) {
                animationTime += (float)time * speed;
                float radius = 2.0f;
                float x, z;
                if (!elipseAnimationOn) { 
                    x = radius * MathF.Sin((float)animationTime);
                    z = radius * MathF.Cos((float)animationTime);
                } else {
                    x = radius / 2f * MathF.Cos((float)animationTime);
                    z = radius * MathF.Sin((float)animationTime);
                }
                shapeModel = Matrix4.CreateTranslation(x, 0.0f, z);
                if (shader != null) {
                    shader.Use();
                    GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "model"), false, ref shapeModel);
                }
            }
        }

        public void Unload() {
            if (shader != null) {
                GL.DeleteBuffer(shapeVBO);
                GL.DeleteVertexArray(shapeVAO);
                GL.DeleteProgram(shader.Handle);
            }
        }

        protected void UpdateVertexBuffer() {
            GL.BindBuffer(BufferTarget.ArrayBuffer, shapeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
        }
    }
}