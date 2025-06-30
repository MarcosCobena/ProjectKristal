using Evergine.Common.IO;
using Evergine.Framework;
using Evergine.Framework.Services;
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
        }

        public override void Initialize()
        {
            base.Initialize();

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();
            var assetsService = this.Container.Resolve<AssetsService>();

            // Navigate to scene
            var scene = assetsService.Load<MyScene>(EvergineContent.Scenes.MyScene_wescene);
#if DEBUG
            var scenes = new Scene[] 
            { 
                // TODO gather classes inheriting from VisualTest automatically
                new BackgroundColorTest(),
                new ModelTest(),
                new EffectTest(),
                new ComputeTest(),
                // We let this at the end so the user can experience the app as it is used to
                scene,
            };

            for (int i = scenes.Length - 1; i >= 0; i--)
            {
                var item = scenes[i];
                var context = new ScreenContext(item);
                screenContextManager.Push(context);
            }
#else
            ScreenContext screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);
#endif
        }
    }
}
