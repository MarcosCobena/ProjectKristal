using Evergine.Common.IO;
using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.VisualTests;
using ProjectKristal.VisualTests;

namespace ProjectKristal
{
    public partial class MyApplication : Application
    {
        public MyApplication()
        {
            this.Container.Register<Settings>();
            this.Container.Register<Clock>();
            this.Container.Register<TimerFactory>();
            this.Container.Register<Random>();
            this.Container.Register<ErrorHandler>();
            this.Container.Register<ScreenContextManager>();
            this.Container.Register<GraphicsPresenter>();
            this.Container.Register<AssetsDirectory>();
            this.Container.Register<AssetsService>();
            this.Container.Register<ForegroundTaskSchedulerService>();
            this.Container.Register<WorkActionScheduler>();
            this.Container.RegisterInstance(new VisualTestSettings());
        }

        public override void Initialize()
        {
            base.Initialize();

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();
            var assetsService = this.Container.Resolve<AssetsService>();

#if DEBUG
            var settings = this.Container.Resolve<VisualTestSettings>();

            Scene[] scenes;
            if (settings.RegenerateScreenshots)
            {
                scenes = new Scene[]
                {
                    // Non-visual base so the last test can pop without leaving an empty stack
                    new RegenerationTerminalScene(),
                    new BackgroundColorTest(),
                    new ModelTest(),
                };
            }
            else
            {
                // Navigate to scene
                var scene = assetsService.Load<MyScene>(EvergineContent.Scenes.MyScene_wescene);
                scenes = new Scene[]
                {
                    // TODO gather classes inheriting from VisualTest automatically
                    new BackgroundColorTest(),
                    new ModelTest(),
                    new EffectTest(),
                    new ComputeTest(),
                    // We let this at the end so the user can experience the app as it is used to
                    scene,
                };
            }

            for (int i = scenes.Length - 1; i >= 0; i--)
            {
                var item = scenes[i];
                var context = new ScreenContext(item);
                screenContextManager.Push(context);
            }
#else
            // Navigate to scene
            var scene = assetsService.Load<MyScene>(EvergineContent.Scenes.MyScene_wescene);
            ScreenContext screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);
#endif
        }
    }
}
