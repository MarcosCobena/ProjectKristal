using Codeuctivity.ImageSharpCompare;
using Evergine.Assets;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;

namespace Evergine.VisualTests
{
    public abstract class VisualTest : Scene
    {
        private readonly GraphicsContext graphicsContext;
        private readonly ScreenContextManager screenContextManager;
        private Camera3D? camera3D;
        private uint frameCount = 0;

        public VisualTest()
        {
            // TODO avoid static Application.Current
            graphicsContext = Application.Current.Container.Resolve<GraphicsContext>();
            screenContextManager = Application.Current.Container.Resolve<ScreenContextManager>();
        }

        public abstract void ArrangeActScene(Entity camera);

        public virtual void Assert()
        {
            // Intentionally empty
        }

        public bool EqualImages()
        {
            var className = GetType().Name;
            const string directoryName = "Screenshots";
            var actualFilePath = Path.Combine(directoryName, $"{className}.png");
            var expectedFilePath = Path.Combine(directoryName, $"{className}-expected.png");

            return ImageSharpCompare.ImagesAreEqual(actualFilePath, expectedFilePath);
        }

        protected override void CreateScene()
        {
            base.CreateScene();
            camera3D = new Camera3D();
            // TODO get actual screen size
            camera3D.FrameBuffer = graphicsContext.Factory.CreateFrameBuffer(1280, 720);
            var camera = new Entity()
                .AddComponent(camera3D)
                .AddComponent(new Transform3D());
            Managers.EntityManager.Add(camera);
            ArrangeActScene(camera);
        }

        protected override void Update(TimeSpan gameTime)
        {
            base.Update(gameTime);
            frameCount++;

            if (frameCount == 2)
            {
                var className = GetType().Name;
                const string directoryName = "Screenshots";
                var filePath = Path.Combine(directoryName, $"{className}.png");

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                camera3D!.FrameBuffer.ColorTargets[0].Texture.SaveToFile(graphicsContext, filePath);
                this.Assert();
                screenContextManager.Pop();
            }
        }
    }
}
