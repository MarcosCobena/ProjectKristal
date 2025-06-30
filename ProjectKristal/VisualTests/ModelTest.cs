using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.VisualTests;

namespace ProjectKristal.VisualTests
{
    internal class ModelTest : VisualTest
    {
        public override void ArrangeActAssertScene(Entity camera)
        {
            var cameraTransform = camera.FindComponent<Transform3D>();
            cameraTransform.Position = new Vector3(3);
            cameraTransform.LookAt(Vector3.Zero, Vector3.Up);
            var assetsService = Application.Current.Container.Resolve<AssetsService>();
            // TODO disable lighting
            var material = assetsService.Load<Material>(EvergineContent.Materials.DefaultMaterial);
            var model = new Entity()
                .AddComponent(new TeapotMesh())
                .AddComponent(new MeshRenderer())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new Transform3D());
            Managers.EntityManager.Add(model);
        }
    }
}
