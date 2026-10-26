using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Evergine.Common.Graphics;

namespace Evergine.VisualTests
{
    internal static class PngSaver
    {
        public static byte[] ReadRgbaPixels(GraphicsContext graphicsContext, Texture source)
        {
            var desc = source.Description;
            var staging = CreateStagingTexture(graphicsContext, source);
            try
            {
                CopyAndSubmit(graphicsContext, source, staging);
                return MapRead(graphicsContext, staging, desc.Width, desc.Height);
            }
            finally
            {
                staging.Dispose();
            }
        }

        public static void SaveAsPng(GraphicsContext graphicsContext, Texture source, string filePath)
        {
            var pixels = ReadRgbaPixels(graphicsContext, source);
            WritePng(pixels, (int)source.Description.Width, (int)source.Description.Height, filePath);
        }

        private static Texture CreateStagingTexture(GraphicsContext graphicsContext, Texture source)
        {
            var sourceDesc = source.Description;
            var stagingDesc = new TextureDescription
            {
                Width = sourceDesc.Width,
                Height = sourceDesc.Height,
                Depth = sourceDesc.Depth,
                ArraySize = sourceDesc.ArraySize,
                MipLevels = 1,
                Format = sourceDesc.Format,
                Usage = ResourceUsage.Staging,
                CpuAccess = ResourceCpuAccess.Read,
                Flags = TextureFlags.None,
                Type = sourceDesc.Type,
                SampleCount = TextureSampleCount.None,
            };
            return graphicsContext.Factory.CreateTexture(ref stagingDesc, "VisualTestPngSaverStaging");
        }

        private static void CopyAndSubmit(GraphicsContext graphicsContext, Texture source, Texture staging)
        {
            var queue = graphicsContext.Factory.CreateCommandQueue(CommandQueueType.Graphics);
            var command = queue.CommandBuffer();
            command.Begin();
            command.CopyTextureDataTo(source, staging);
            command.End();
            command.Commit();
            queue.Submit();
            queue.WaitIdle();
        }

        private static byte[] MapRead(GraphicsContext graphicsContext, Texture staging, uint width, uint height)
        {
            var mapped = graphicsContext.MapMemory(staging, MapMode.Read, subResource: 0);
            try
            {
                var pixels = new byte[width * height * 4];
                var rowBytes = width * 4u;
                var sourceBytes = mapped.RowPitch < rowBytes ? rowBytes : mapped.RowPitch;
                for (uint y = 0; y < height; y++)
                {
                    Marshal.Copy(
                        source: mapped.Data + (int)(y * sourceBytes),
                        destination: pixels,
                        startIndex: (int)(y * rowBytes),
                        length: (int)rowBytes);
                }
                return pixels;
            }
            finally
            {
                graphicsContext.UnmapMemory(staging, subResource: 0);
            }
        }

        private static void WritePng(byte[] rgbaPixels, int width, int height, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using var bitmap = new SkiaSharp.SKBitmap(width, height, SkiaSharp.SKColorType.Rgba8888, SkiaSharp.SKAlphaType.Unpremul);
            var span = bitmap.GetPixelSpan();
            rgbaPixels.AsSpan().CopyTo(span);
            using var img = SkiaSharp.SKImage.FromBitmap(bitmap);
            using var data = img.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            using var stream = File.Create(filePath);
            data.SaveTo(stream);
        }
    }
}
