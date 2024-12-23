using OpenTK.Windowing.Desktop;

namespace lab4 {
    public static class Program {
        [Obsolete]
        static void Main() {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings() {
                Size = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "3D"
            };
            
            using var window = new Window(gameWindowSettings, nativeWindowSettings);
            window.Run();
        }
    }
}