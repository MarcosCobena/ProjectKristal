using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.VisualTests;
using Xunit;

namespace ProjectKristal.VisualTests
{
    internal class BackgroundColorTest : VisualTest
    {
        public override void ArrangeActAssertScene(Entity camera)
        {
            var camera3D = camera.FindComponent<Camera3D>();
            camera3D.BackgroundColor = Color.Fuchsia;
            Assert.True(this.EqualImages());
            // TODO @jcanton: Assert.EqualColorRGB/A(expectedColor, x, y);
        }
    }
}
