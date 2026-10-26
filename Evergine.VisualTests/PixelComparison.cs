using System;
using System.IO;

namespace Evergine.VisualTests
{
    internal static class PixelComparison
    {
        public static bool AreEqual(byte[] actual, byte[] expected, int width, int height)
        {
            var expectedCount = width * height * 4;
            if (actual.Length != expectedCount) return false;
            if (expected.Length != expectedCount) return false;
            return actual.AsSpan().SequenceEqual(expected);
        }

        public static byte[] LoadExpectedRgba(string pngPath)
        {
            using var stream = File.OpenRead(pngPath);
            using var bitmap = SkiaSharp.SKBitmap.Decode(stream);
            if (bitmap == null)
            {
                throw new InvalidOperationException("Unable to decode expected screenshot: " + pngPath);
            }

            var width = bitmap.Width;
            var height = bitmap.Height;
            var result = new byte[width * height * 4];

            using var rgba = new SkiaSharp.SKBitmap(width, height, SkiaSharp.SKColorType.Rgba8888, SkiaSharp.SKAlphaType.Unpremul);
            if (!bitmap.CopyTo(rgba, SkiaSharp.SKColorType.Rgba8888))
            {
                throw new InvalidOperationException("Unable to convert expected screenshot to RGBA8: " + pngPath);
            }
            rgba.GetPixelSpan().CopyTo(result);
            return result;
        }
    }
}
