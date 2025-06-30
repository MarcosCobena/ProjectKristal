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
        public override void ArrangeActScene(Entity camera)
        {
            var cameraTransform = camera.FindComponent<Transform3D>();
            cameraTransform.Position = new Vector3(3);
            cameraTransform.LookAt(Vector3.Zero, Vector3.Up);
            var assetsService = Application.Current.Container.Resolve<AssetsService>();
            // TODO disable lighting
            // FIXME EvergineContent.Materials.DefaultMaterial returns Guid.Empty;
            // may it be because this scene is under a directory?
            var material = assetsService.Load<Material>("02181b63-5a0e-46d1-9208-d92376ae33fb");
            var model = new Entity()
                .AddComponent(new TeapotMesh())
                .AddComponent(new MeshRenderer())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new Transform3D());
            Managers.EntityManager.Add(model);
        }
    }
}
