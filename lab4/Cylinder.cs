using OpenTK.Mathematics;

namespace lab4
{
    public class Cylinder : Shape {

        private readonly int segmentCount = 32;
        private readonly float radius = 0.3f;
        private readonly float height = 0.6f;

        private readonly List<float> cylVertices = [];
        private readonly List<uint> cylIndices = [];

        public Cylinder(Shader shader, Camera camera) : base(shader, camera) {
            GenerateCylinder();
            Initialize([.. cylVertices], [.. cylIndices]);
        }

        private void GenerateCylinder() {
            for (int i = 0; i <= segmentCount; i++) {
                float angle = (float)i / segmentCount * MathHelper.TwoPi;
                float x = radius * MathF.Cos(angle);
                float z = radius * MathF.Sin(angle);

                // Нижнее основание
                cylVertices.Add(x);
                cylVertices.Add(-height / 2);
                cylVertices.Add(z);

                // нормали
                cylVertices.Add(x / radius);
                cylVertices.Add(0);
                cylVertices.Add(z / radius);

                // Верхнее основание
                cylVertices.Add(x);
                cylVertices.Add(height / 2);
                cylVertices.Add(z);

                // нормали
                cylVertices.Add(x / radius);
                cylVertices.Add(0);
                cylVertices.Add(z / radius);
            }

            for (int i = 0; i <= segmentCount; i++) {
                float angle = (float)i / segmentCount * MathHelper.TwoPi;
                float x = radius * MathF.Cos(angle);
                float z = radius * MathF.Sin(angle);

                // Вершина нижнего основания
                cylVertices.Add(x);
                cylVertices.Add(-height / 2);
                cylVertices.Add(z);

                // Нормаль для нижнего основания
                cylVertices.Add(0);
                cylVertices.Add(-1);
                cylVertices.Add(0);
            }

            for (int i = 0; i <= segmentCount; i++) {
                float angle = (float)i / segmentCount * MathHelper.TwoPi;
                float x = radius * MathF.Cos(angle);
                float z = radius * MathF.Sin(angle);

                // Вершина верхнего основания
                cylVertices.Add(x);
                cylVertices.Add(height / 2);
                cylVertices.Add(z);

                // Нормаль для верхнего основания
                cylVertices.Add(0);
                cylVertices.Add(1);
                cylVertices.Add(0);
            }

            // Генерация индексов для боковой поверхности
            for (int i = 0; i < segmentCount; i++) {
                cylIndices.Add((uint)(i * 2));
                cylIndices.Add((uint)((i + 1) % segmentCount * 2));
                cylIndices.Add((uint)(i * 2 + 1));

                cylIndices.Add((uint)((i + 1) % segmentCount * 2));
                cylIndices.Add((uint)((i + 1) % segmentCount * 2 + 1));
                cylIndices.Add((uint)(i * 2 + 1));
            }

            // Индексы для нижнего основания
            int bottomCenterIndex = (segmentCount + 1) * 2;
            for (int i = 0; i < segmentCount; i++) {
                cylIndices.Add((uint)bottomCenterIndex);
                cylIndices.Add((uint)(bottomCenterIndex + i + 1));
                cylIndices.Add((uint)(bottomCenterIndex + (i + 1) % segmentCount + 1));
            }

            // Индексы для верхнего основания
            int topCenterIndex = bottomCenterIndex + segmentCount + 2;
            for (int i = 0; i < segmentCount; i++) {
                cylIndices.Add((uint)topCenterIndex);
                cylIndices.Add((uint)(topCenterIndex + (i + 1) % segmentCount + 1));
                cylIndices.Add((uint)(topCenterIndex + i + 1));
            }
        }
    }
}
