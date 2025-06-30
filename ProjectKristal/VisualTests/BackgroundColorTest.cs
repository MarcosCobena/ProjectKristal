using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.VisualTests;
using Xunit;

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
            // FIXME it compares the screenshot from previous run, since current is taken after in Update()
            Xunit.Assert.True(this.EqualImages());
            // TODO @jcanton: Assert.EqualColorRGB/A(expectedColor, x, y);
        }
    }
}
