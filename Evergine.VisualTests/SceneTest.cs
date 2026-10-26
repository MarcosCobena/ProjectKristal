using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;

namespace Evergine.VisualTests
{
    public abstract class SceneTest : Scene
    {
        private static int s_activeSceneTestCount;
        private readonly GraphicsContext graphicsContext;
        private readonly ScreenContextManager screenContextManager;
        private Camera3D? camera3D;
        private uint frameCount = 0;

        protected GraphicsContext GraphicsContext => graphicsContext;
        protected Camera3D? Camera3D => camera3D;

        public SceneTest()
        {
            s_activeSceneTestCount++;
            graphicsContext = Application.Current.Container.Resolve<GraphicsContext>();
            screenContextManager = Application.Current.Container.Resolve<ScreenContextManager>();
            screenContextManager.OnDesactivatingScene += OnDesactivatingScene;
        }

        public abstract void ArrangeActScene(Entity camera);

        public virtual void SaveCapture()
        {
        }

        public virtual void Assert()
        {
        }

        protected static bool IsRegenerateMode()
        {
            return Application.Current.Container.Resolve<VisualTestSettings>().RegenerateScreenshots;
        }

        public static bool IsAnyActive() => s_activeSceneTestCount > 0;

        public static readonly System.TimeSpan FixedTimeStep = System.TimeSpan.FromSeconds(1.0 / 60.0);

        private void OnDesactivatingScene(Scene scene)
        {
            if (scene is SceneTest)
            {
                s_activeSceneTestCount--;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                screenContextManager.OnDesactivatingScene -= OnDesactivatingScene;
            }
        }

        protected override void CreateScene()
        {
            base.CreateScene();
            camera3D = new Camera3D();
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
                this.SaveCapture();
                if (!IsRegenerateMode())
                {
                    this.Assert();
                }
                screenContextManager.Pop();
            }
        }
    }
}
