using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace lab3 {
    public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings) {
        
        private readonly Cube cube = new();
        private readonly Background background = new("resources/image.png");
        private const double TargetFrameTime = 1.0 / 60.0;

        protected override void OnLoad() {
            base.OnLoad();
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            cube.Load(Size.X, Size.Y);
            cube.Move(new(0f, 0f, -2f));
            background.Load(Size.X, Size.Y);
            background.shape = cube;
        }

        protected override void OnUnload() {
            cube.Unload();
            background.Unload();

            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e) {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            double frameTime = e.Time;
            background.Draw();
            cube.Draw(e.Time);

            SwapBuffers();
            if (frameTime < TargetFrameTime) {
                Thread.Sleep((int)((TargetFrameTime - frameTime) * 1000.0));  // Задержка для достижения целевого FPS
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput() {
            var input = KeyboardState;
            if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space) && !input.WasKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space)) {
                cube.ChangeAnimationStatus();
            } else if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up) && !input.WasKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up)) {
                cube.ChangeAnimationType();
            } else if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W) && !input.WasKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
                cube.IncreaseSpeed();
            } else if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S) && !input.WasKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
                cube.DecreaseSpeed();
            }
        }
    }
}