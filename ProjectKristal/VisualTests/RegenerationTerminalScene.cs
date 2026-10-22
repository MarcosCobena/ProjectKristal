using System;
using Evergine.Framework;

namespace ProjectKristal.VisualTests
{
    internal class RegenerationTerminalScene : Scene
    {
        private bool exited;

        protected override void Update(TimeSpan gameTime)
        {
            base.Update(gameTime);
            if (this.exited)
            {
                return;
            }
            this.exited = true;
            Environment.Exit(0);
        }
    }
}
