using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Graphics.Effects;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.VisualTests;

namespace ProjectKristal.VisualTests
{
    internal class EffectTest : VisualTest
    {
        public override void ArrangeActScene(Entity camera)
        {
            var graphicsContext = Application.Current.Container.Resolve<GraphicsContext>();
            var shader = @"
[Begin_ResourceLayout]

cbuffer PerDrawCall : register(b0)
{
    float4x4 WorldViewProj : packoffset(c0); [WorldViewProjection]
};

[End_ResourceLayout]

[Begin_Pass:Default]
[Profile 10_0]
[Entrypoints VS=VS PS=PS]

struct VS_IN
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float3 Nor : NORMAL;
    float2 Tex : TEXCOORD;
};

PS_IN VS(VS_IN input)
{
    PS_IN output = (PS_IN)0;

    output.pos = mul(input.Position, WorldViewProj);
    output.Nor = input.Normal;
    output.Tex = input.TexCoord;

    return output;
}

float4 PS(PS_IN input) : SV_Target
{
    return float4(1, 0, 1, 1);
}

[End_Pass]
";
            var effect = new EffectFromCode(graphicsContext, shader);
            var assetsService = Application.Current.Container.Resolve<AssetsService>();
            var material = new Material(effect)
            {
                LayerDescription = assetsService.Load<RenderLayerDescription>(EvergineContent.RenderLayers.Opaque),
            };
            var cameraTransform = camera.FindComponent<Transform3D>();
            cameraTransform.Position = new Vector3(3);
            cameraTransform.LookAt(Vector3.Zero, Vector3.Up);
            var primitive = new Entity()
                .AddComponent(new SphereMesh())
                .AddComponent(new MeshRenderer())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new Transform3D());
            this.Managers.EntityManager.Add(primitive);
        }
    }
}
