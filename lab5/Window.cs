using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RayTracing {
    public class Window : GameWindow {
        private Shader _shader;
        private Camera _camera;
        private const double TargetFrameTime = 1.0 / 60.0;
        private bool firstMove = true;
        private System.Numerics.Vector2 lastPos;

        private float[] _vertices = {
            -1f, -1f, 0f,
            -1f, 1f, 0f,
            1f, -1f, 0f,
            1f, 1f, 0f
        };
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int maxDepthTrace = 1;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
            _shader = new("shaders/shader.vert", "shaders/shader.frag");
            OpenTK.Mathematics.Vector3 cameraPosition = new(0f, 0f, -7f);
            _camera = new(cameraPosition, Size.X / (float)Size.Y);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnLoad() {
            base.OnLoad();
            _shader.Use();

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (sizeof(float) * _vertices.Length), _vertices, BufferUsageHint.StaticDraw);

            var posLoc = _shader.GetAttribLocation("vPosition");
            GL.EnableVertexAttribArray(posLoc);
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);

            _shader.SetVector3("uCamera.Position", _camera.Position);
            _shader.SetVector3("uCamera.View", _camera.Front);
            _shader.SetVector3("uCamera.Up", _camera.Up);
            _shader.SetVector3("uCamera.Side", _camera.Right);
            _shader.SetVector2("uCamera.Scale", new Vector2(_camera.AspectRatio));
            _shader.SetInt("MAX_TRACE_DEPTH", maxDepthTrace);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnResize(ResizeEventArgs e) {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);
            Title = $"CG Lab03 FPS: {1f / e.Time:0}";

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindVertexArray(_vertexArrayObject);

            _shader.Use();
            _shader.SetVector3("uCamera.Position", _camera.Position);
            _shader.SetVector3("uCamera.View", _camera.Front);
            _shader.SetVector3("uCamera.Up", _camera.Up);
            _shader.SetVector3("uCamera.Side", _camera.Right);
            _shader.SetVector2("uCamera.Scale", new Vector2(_camera.AspectRatio));

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            SwapBuffers();
            double frameTime = e.Time;
            if (frameTime < TargetFrameTime) {
                Thread.Sleep((int)((TargetFrameTime - frameTime) * 1000.0));  // Задержка для достижения целевого FPS
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            HandleKeyboardInput(e);
        }

        private void HandleKeyboardInput(FrameEventArgs e) {
            var input = KeyboardState;
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)) {
                Close();
            }
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space)) {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift)) {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up)) {
                maxDepthTrace++;
                _shader.SetInt("MAX_TRACE_DEPTH", maxDepthTrace);
            }
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down)) {
                maxDepthTrace--;
                _shader.SetInt("MAX_TRACE_DEPTH", maxDepthTrace);
            }

            var mouse = MouseState;

            if (firstMove) {
                lastPos = new(mouse.X, mouse.Y);
                firstMove = false;
            }
            else {
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnUnload() {
            GL.DeleteProgram(_shader.Handle);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.UseProgram(0);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
            base.OnUnload();
        }
    }
}