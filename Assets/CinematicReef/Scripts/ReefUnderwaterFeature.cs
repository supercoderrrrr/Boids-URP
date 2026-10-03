using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CinematicReef
{
    public sealed class ReefUnderwaterFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader shader;
        private Material material;
        private WaterPass pass;
        public void Configure(Shader value) { shader = value; Create(); }
        public override void Create()
        {
            pass?.Dispose(); CoreUtils.Destroy(material);
            material = shader == null ? null : CoreUtils.CreateEngineMaterial(shader);
            pass = new WaterPass(material) { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (material != null && data.cameraData.cameraType == CameraType.Game) renderer.EnqueuePass(pass);
        }
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData data)
        {
            if (data.cameraData.cameraType == CameraType.Game) pass.Source = renderer.cameraColorTargetHandle;
        }
        protected override void Dispose(bool disposing) { pass?.Dispose(); CoreUtils.Destroy(material); }
        private sealed class WaterPass : ScriptableRenderPass
        {
            private readonly Material material;
            private RTHandle light, composite;
            private readonly ProfilingSampler sampler = new ProfilingSampler("Reef underwater and shadowed scattering");
            public RTHandle Source;
            public WaterPass(Material value) { material = value; ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color); }
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
            {
                var desc = data.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0; desc.msaaSamples = 1;
                RenderingUtils.ReAllocateIfNeeded(ref composite, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_ReefComposite");
                desc.width = Mathf.Max(1, desc.width / 2); desc.height = Mathf.Max(1, desc.height / 2);
                desc.colorFormat = RenderTextureFormat.ARGBHalf;
                RenderingUtils.ReAllocateIfNeeded(ref light, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_ReefScattering");
            }
            public override void Execute(ScriptableRenderContext context, ref RenderingData data)
            {
                if (Source == null || material == null) return;
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, sampler))
                {
                    Blitter.BlitCameraTexture(cmd, Source, light, material, 0);
                    cmd.SetGlobalTexture("_ReefScattering", light.nameID);
                    cmd.SetGlobalVector("_ReefScattering_TexelSize", new Vector4(1f / light.rt.width, 1f / light.rt.height, light.rt.width, light.rt.height));
                    Blitter.BlitCameraTexture(cmd, Source, composite, material, 1);
                    Blitter.BlitCameraTexture(cmd, composite, Source);
                }
                context.ExecuteCommandBuffer(cmd); CommandBufferPool.Release(cmd);
            }
            public void Dispose() { light?.Release(); composite?.Release(); Source = null; }
        }
    }
}
