using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RcPauseBlurFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader blurShader;
    [Range(1, 6)]
    [SerializeField] private int iterations = 3;

    public static RcPauseBlurFeature Instance { get; private set; }
    public float BlurAmount { get; set; }

    private RcPauseBlurPass blurPass;
    private Material blurMaterial;

    public override void Create()
    {
        Instance = this;
        if (blurShader == null) return;

        blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);
        blurPass = new RcPauseBlurPass(blurMaterial, iterations)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blurMaterial == null || blurPass == null || BlurAmount <= 0f) return;
        blurPass.Setup(BlurAmount);
        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(blurMaterial);
    }
}

public class RcPauseBlurPass : ScriptableRenderPass
{
    private readonly Material material;
    private readonly int iterations;
    private float blurAmount;

    private static readonly int BlurSizeId     = Shader.PropertyToID("_BlurSize");
    private static readonly int BlitTextureId   = Shader.PropertyToID("_BlitTexture");

    private class PassData
    {
        public TextureHandle source;
        public TextureHandle temp;
        public Material      material;
        public float         blurSize;
        public int           iterations;
    }

    public RcPauseBlurPass(Material material, int iterations)
    {
        this.material   = material;
        this.iterations = iterations;
    }

    public void Setup(float amount) => blurAmount = amount;

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (material == null || blurAmount <= 0f) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraColor  = resourceData.activeColorTexture;

        var desc = renderGraph.GetTextureDesc(cameraColor);
        desc.name        = "_PauseBlurTemp";
        desc.clearBuffer = false;
        var tempHandle = renderGraph.CreateTexture(desc);

        using (var builder = renderGraph.AddUnsafePass<PassData>("RcPauseBlur", out var passData))
        {
            passData.source     = cameraColor;
            passData.temp       = tempHandle;
            passData.material   = material;
            passData.blurSize   = blurAmount;
            passData.iterations = iterations;

            builder.UseTexture(cameraColor, AccessFlags.ReadWrite);
            builder.UseTexture(tempHandle,  AccessFlags.ReadWrite);

            builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
            {
                data.material.SetFloat(BlurSizeId, data.blurSize);

                for (int i = 0; i < data.iterations; i++)
                {
                    // Horizontal: source → temp
                    ctx.cmd.SetGlobalTexture(BlitTextureId, data.source);
                    ctx.cmd.SetRenderTarget(data.temp);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);

                    // Vertical: temp → source
                    ctx.cmd.SetGlobalTexture(BlitTextureId, data.temp);
                    ctx.cmd.SetRenderTarget(data.source);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.material, 1, MeshTopology.Triangles, 3);
                }
            });
        }
    }
}
