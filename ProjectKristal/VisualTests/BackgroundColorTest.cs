using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.VisualTests;

namespace ProjectKristal.VisualTests
{
    internal class BackgroundColorTest : VisualTest
    {
        public override void ArrangeActScene(Entity camera)
        {
            var camera3D = camera.FindComponent<Camera3D>();
            camera3D.BackgroundColor = Color.Fuchsia;
        }

        public override void Assert()
        {
            Xunit.Assert.True(this.EqualImages());
            // TODO @jcanton: Assert.EqualColorRGB/A(expectedColor, x, y);
        }
    }
}
