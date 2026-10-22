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
                    new BackgroundColorTest(),
                    new ModelTest(),
                    // FIXME
                    //System.InvalidOperationException
                    //  HResult=0x80131509
                    //  Mensaje = You can not pop when only exists one sceneContext
                    //  Origen = Evergine.Framework
                    //  Seguimiento de la pila:
                    //   en Evergine.Framework.Services.ScreenContextManager.PopExecute(ScreenTransition transition, Boolean doDispose)
                    //   en Evergine.Framework.ScreenContextCommands.PopContextCommand.Execute()
                    //   en Evergine.Framework.Services.ScreenContextManager.Update(TimeSpan gameTime)
                    //   en Evergine.Framework.Application.UpdateFrame(TimeSpan gameTime)
                    //   en ProjectKristal.Windows.Program.<>c__DisplayClass5_0.<Main>b__1() en C:\Repositorios\ProjectKristal\ProjectKristal.Windows\Program.cs: línea 75
                    //   en Evergine.Forms.RenderLoop.Run(Control form, Action renderCallback, Boolean useApplicationDoEvents)
                    //   en Evergine.Forms.FormsWindowsSystem.CreateLoopThread(Action loadAction, Action renderCallback)
                    //   en Evergine.Common.Graphics.WindowsSystem.Run(Action loadAction, Action renderCallback)
                    //   en ProjectKristal.Windows.Program.Main(String[] args) en C:\Repositorios\ProjectKristal\ProjectKristal.Windows\Program.cs: línea 65
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
