using System.Reflection;
using Codeuctivity.ImageSharpCompare;
using Evergine.Assets;
using Evergine.Framework;

namespace Evergine.VisualTests
{
    public abstract class VisualTest : SceneTest
    {
        public sealed override void SaveCapture()
        {
            var className = GetType().Name;

            if (IsRegenerateMode())
            {
                var sourceDir = GetScreenshotsSourceDir();
                Directory.CreateDirectory(sourceDir);
                var expectedFilePath = Path.Combine(sourceDir, $"{className}-expected.png");
                Camera3D!.FrameBuffer.ColorTargets[0].Texture.SaveToFile(GraphicsContext, expectedFilePath);
                return;
            }

            const string directoryName = "Screenshots";
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            var actualFilePath = Path.Combine(directoryName, $"{className}.png");
            Camera3D!.FrameBuffer.ColorTargets[0].Texture.SaveToFile(GraphicsContext, actualFilePath);
        }

        public bool EqualImages()
        {
            var className = GetType().Name;
            const string directoryName = "Screenshots";
            var actualFilePath = Path.Combine(directoryName, $"{className}.png");
            var expectedFilePath = Path.Combine(directoryName, $"{className}-expected.png");

            return ImageSharpCompare.ImagesAreEqual(actualFilePath, expectedFilePath);
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
