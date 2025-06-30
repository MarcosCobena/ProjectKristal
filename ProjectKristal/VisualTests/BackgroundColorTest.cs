using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.VisualTests;

namespace ProjectKristal.VisualTests
{
    internal class BackgroundColorTest : VisualTest
    {
        public override void ArrangeActAssertScene(Entity camera)
        {
            var camera3D = camera.FindComponent<Camera3D>();
            camera3D.BackgroundColor = Color.Fuchsia;
            // TODO @jcanton: Assert.EqualPixels();
            // TODO @jcanton: Assert.EqualColorRGB/A(expectedColor, x, y);
        }
    }
}
