using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BoidsUnderwaterScene
{
    [ExecuteAlways]
    public sealed class UnderwaterEnvironment : MonoBehaviour
    {
        [SerializeField] private UniversalRenderPipelineAsset pipeline;
        [SerializeField] private Light sun;
        [SerializeField] private float surfaceHeight = 28f;
        [Header("Underwater optics")]
        [SerializeField] private Vector3 absorption = new Vector3(.026f,.017f,.013f);
        [SerializeField] private Color scatteringColor = new Color(.018f,.19f,.235f);
        [SerializeField,Range(.001f,.1f)] private float scatteringDensity = .015f;
        [SerializeField,Range(0,.06f)] private float lightShaftIntensity = .028f;
        [SerializeField,Range(0,.003f)] private float distortion = .00045f;
        [SerializeField,Range(0,3)] private float chromaticSeparationPixels = 1.1f;
        [SerializeField,Range(0,1)] private float caustics = .2f;
        private RenderPipelineAsset previousQuality;
        public float SurfaceHeight => surfaceHeight;
        private void OnEnable()
        {
            previousQuality = QualitySettings.renderPipeline;
            if (pipeline != null) QualitySettings.renderPipeline = pipeline;
            Update();
        }
        private void Update()
        {
            Shader.SetGlobalFloat("_UnderwaterSurfaceHeight",surfaceHeight);
            Shader.SetGlobalVector("_UnderwaterAbsorption",absorption);
            Shader.SetGlobalColor("_UnderwaterFogTint",scatteringColor);
            Shader.SetGlobalFloat("_UnderwaterFogDensity",scatteringDensity);
            Shader.SetGlobalFloat("_UnderwaterShaftIntensity",lightShaftIntensity);
            Shader.SetGlobalFloat("_UnderwaterDistortion",distortion);
            Shader.SetGlobalFloat("_UnderwaterChromaticPixels",chromaticSeparationPixels);
            Shader.SetGlobalFloat("_UnderwaterCaustics",caustics);
        }
        private void OnDisable()
        {
            if (QualitySettings.renderPipeline == pipeline) QualitySettings.renderPipeline = previousQuality;
        }
    }
}
