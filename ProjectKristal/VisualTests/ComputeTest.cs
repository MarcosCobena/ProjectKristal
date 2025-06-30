using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics.Effects;
using Evergine.Framework.Services;
using Evergine.VisualTests;
using System.Runtime.CompilerServices;
using Xunit;

namespace ProjectKristal.VisualTests
{
    internal class ComputeTest : VisualTest
    {
        // TODO @jcanton: encapsulate boilerplate code to make test smaller
        public unsafe override void ArrangeActAssertScene(Entity camera)
        {
            // Arrange
            var assetsService = Application.Current.Container.Resolve<AssetsService>();
            var effect = assetsService.Load<Effect>(EvergineContent.VisualTests.MyComputeEffect);
            var task = new MyComputeEffect(effect);
            var bufferDescription = new BufferDescription()
            {
                CpuAccess = ResourceCpuAccess.None,
                Flags = BufferFlags.UnorderedAccess | BufferFlags.ShaderResource | BufferFlags.BufferStructured,
                SizeInBytes = (uint)Unsafe.SizeOf<uint>(),
                StructureByteStride = Unsafe.SizeOf<uint>(),
                Usage = ResourceUsage.Default,
            };
            var graphicsContext = Application.Current.Container.Resolve<GraphicsContext>();
            var buffer = graphicsContext.Factory.CreateBuffer(ref bufferDescription);
            task.Buffer0 = buffer;

            // Act
            task.Run2D(threadCountX: 1, threadCountY: 1);

            // Assert
            var exportBufferDescription = new BufferDescription()
            {
                CpuAccess = ResourceCpuAccess.Read,
                Flags = BufferFlags.None,
                SizeInBytes = (uint)Unsafe.SizeOf<uint>(),
                StructureByteStride = 0,
                Usage = ResourceUsage.Staging,
            };
            var exportBuffer = graphicsContext.Factory.CreateBuffer(ref exportBufferDescription);
            var exportQueue = graphicsContext.Factory.CreateCommandQueue(CommandQueueType.Compute);
            var command = exportQueue.CommandBuffer();
            command.Begin();
            command.CopyBufferDataTo(buffer, exportBuffer, (uint)Unsafe.SizeOf<uint>());
            command.End();
            command.Commit();
            exportQueue.Submit();
            exportQueue.WaitIdle();
            var mappedResource = graphicsContext.MapMemory(exportBuffer, MapMode.Read);
            var pointer = (uint*)mappedResource.Data;
            Assert.Equal(1u, pointer[0]);
        }
    }
}
