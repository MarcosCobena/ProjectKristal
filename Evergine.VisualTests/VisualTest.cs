using System.Reflection;

namespace Evergine.VisualTests
{
    public abstract class VisualTest : SceneTest
    {
        public sealed override void SaveCapture()
        {
            var className = GetType().Name;
            var colorTexture = Camera3D!.FrameBuffer.ColorTargets[0].Texture;

            if (IsRegenerateMode())
            {
                var sourceDir = GetScreenshotsSourceDir();
                Directory.CreateDirectory(sourceDir);
                var expectedFilePath = Path.Combine(sourceDir, $"{className}-expected.png");
                PngSaver.SaveAsPng(GraphicsContext, colorTexture, expectedFilePath);
                return;
            }

            const string directoryName = "Screenshots";
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            var actualFilePath = Path.Combine(directoryName, $"{className}.png");
            PngSaver.SaveAsPng(GraphicsContext, colorTexture, actualFilePath);
        }

        public bool EqualImages()
        {
            var className = GetType().Name;
            const string directoryName = "Screenshots";
            var actualFilePath = Path.Combine(directoryName, $"{className}.png");
            var expectedFilePath = Path.Combine(directoryName, $"{className}-expected.png");

            var colorTexture = Camera3D!.FrameBuffer.ColorTargets[0].Texture;
            var width = (int)colorTexture.Description.Width;
            var height = (int)colorTexture.Description.Height;

            var actual = PngSaver.ReadRgbaPixels(GraphicsContext, colorTexture);
            var expected = PixelComparison.LoadExpectedRgba(expectedFilePath);
            return PixelComparison.AreEqual(actual, expected, width, height);
        }

        private static string GetScreenshotsSourceDir()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            foreach (var attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
            {
                if (attribute.Key == "ScreenshotsSourceDir")
                {
                    return attribute.Value!;
                }
            }
            const string fallback = "Screenshots";
            return fallback;
        }
    }
}
