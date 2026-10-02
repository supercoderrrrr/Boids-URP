using UnityEngine;

public class UnderwaterFogController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private BoxCollider underwaterVolume;

    [Header("Underwater Fog")]
    [SerializeField] private Color underwaterFogColor = new Color(0.02f, 0.25f, 0.35f);
    [SerializeField] private float underwaterFogDensity = 0.025f;

    private bool defaultFogEnabled;
    private Color defaultFogColor;
    private FogMode defaultFogMode;
    private float defaultFogDensity;

    private void Start()
    {
        defaultFogEnabled = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogMode = RenderSettings.fogMode;
        defaultFogDensity = RenderSettings.fogDensity;
    }

    private void Update()
    {
        if (cameraTransform == null || underwaterVolume == null)
        {
            return;
        }

        bool isUnderwater = IsInsideBox(cameraTransform.position, underwaterVolume);

        if (isUnderwater)
        {
            ApplyUnderwaterFog();
        }
        else
        {
            RestoreDefaultFog();
        }
    }

    private bool IsInsideBox(Vector3 worldPoint, BoxCollider box)
    {
        Vector3 localPoint = box.transform.InverseTransformPoint(worldPoint);
        localPoint -= box.center;

        Vector3 halfSize = box.size * 0.5f;

        return Mathf.Abs(localPoint.x) <= halfSize.x
            && Mathf.Abs(localPoint.y) <= halfSize.y
            && Mathf.Abs(localPoint.z) <= halfSize.z;
    }

    private void ApplyUnderwaterFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = underwaterFogColor;
        RenderSettings.fogDensity = underwaterFogDensity;
    }

    private void RestoreDefaultFog()
    {
        RenderSettings.fog = defaultFogEnabled;
        RenderSettings.fogMode = defaultFogMode;
        RenderSettings.fogColor = defaultFogColor;
        RenderSettings.fogDensity = defaultFogDensity;
    }
}