using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Numerics;

namespace lab4 {
    public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings) {
        private Camera? camera;
        private Shader? lightingShader;
        private const double TargetFrameTime = 1.0 / 60.0;
        private Cylinder? cylinder;
        private bool firstMove = true;

        private System.Numerics.Vector2 lastPos;

        protected override void OnLoad() {
            base.OnLoad();
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            lightingShader = new("shaders/shader.vert", "shaders/lighting.frag");
            OpenTK.Mathematics.Vector3 cameraPosition = new(0f, 0f, 3f);
            camera = new Camera(cameraPosition, Size.X / (float)Size.Y);
            cylinder = new(lightingShader, camera);

            cylinder.Load();
            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload() {
            cylinder?.Unload();
            if (lightingShader != null) GL.DeleteProgram(lightingShader.Handle);
            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e) {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            if (camera != null) {
                camera.AspectRatio = Size.X / (float)Size.Y;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            base.OnMouseWheel(e);
            if (camera != null) {
                camera.Fov -= e.OffsetY;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            double frameTime = e.Time;

            cylinder?.Render();

            SwapBuffers();
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

            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
                camera.Position += camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
                camera.Position -= camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) {
                camera.Position -= camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) {
                camera.Position += camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space)) {
                camera.Position += camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift)) {
                camera.Position -= camera.Up * cameraSpeed * (float)e.Time; // Down
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up)) {
                cylinder?.IncreaseLightingStrength();
            }
            if (camera != null && input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down)) {
                cylinder?.DecreaseLightingStrength();
            }

            var mouse = MouseState;

            if (firstMove) {
                lastPos = new(mouse.X, mouse.Y);
                firstMove = false;
            }
            else if (camera != null) {
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new(mouse.X, mouse.Y);

                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }
        }
    }
}