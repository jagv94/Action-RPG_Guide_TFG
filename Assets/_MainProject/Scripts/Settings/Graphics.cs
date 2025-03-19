using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Graphics : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    // Calidad
    public TMP_Text qualityText;
    private readonly string[] qualityOptions = { "Baja", "Media", "Alta" };
    private int qualityIndex = 2;

    // Brillo y contraste
    public Slider brightnessSlider;
    public TMP_Text brightnessText;
    public Slider contrastSlider;
    public TMP_Text contrastText;

    // Distancia de renderizado
    public TMP_Text renderDistanceText;
    private readonly string[] renderDistanceOptions = { "Cercana", "Media", "Lejana" };
    private int renderDistanceIndex = 2;

    // Calidad de sombras
    public TMP_Text shadowQualityText;
    private readonly string[] shadowQualityOptions = { "Baja", "Media", "Alta" };
    private int shadowQualityIndex = 2;

    // Reflejos
    public TMP_Text reflectionsText;
    private bool reflectionsEnabled = true;

    // Foveated Rendering
    public TMP_Text foveatedRenderingText;
    private readonly string[] foveatedOptions = { "Desactivado", "Estático", "Dinámico" };
    private int foveatedIndex = 0;

    // Occlusion Culling
    public TMP_Text occlusionCullingText;
    private bool occlusionCullingEnabled = true;

    private bool isAdjustingBrightness = false;
    private bool isAdjustingContrast = false;

    void Start()
    {
        LoadSettings();
        UpdateUI();
    }

    // === CALIDAD ===
    public void ChangeQuality(int direction)
    {
        qualityIndex = Mathf.Clamp(qualityIndex + direction, 0, qualityOptions.Length - 1);
        QualitySettings.SetQualityLevel(qualityIndex);
        UpdateUI();
        UserEventLogger.Instance.LogEvent("quality_changed", qualityText.text);
    }

    // === BRILLO ===
    public void OnBrightnessChanged()
    {
        RenderSettings.ambientIntensity = brightnessSlider.value / brightnessSlider.maxValue;
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        brightnessText.text = brightnessSlider.value.ToString("F0");
    }

    // === CONTRASTE ===
    public void OnContrastChanged()
    {
        Shader.SetGlobalFloat("_Contrast", contrastSlider.value / contrastSlider.maxValue);
        PlayerPrefs.SetFloat("Contrast", contrastSlider.value);
        contrastText.text = contrastSlider.value.ToString("F0");
    }

    // === DISTANCIA DE RENDERIZADO ===
    public void ChangeRenderDistance(int direction)
    {
        renderDistanceIndex = Mathf.Clamp(renderDistanceIndex + direction, 0, renderDistanceOptions.Length - 1);
        float[] distances = { 50f, 150f, 300f };
        Camera.main.farClipPlane = distances[renderDistanceIndex];
        UpdateUI();
        UserEventLogger.Instance.LogEvent("render_distance_changed", renderDistanceText.text);
    }

    // === SOMBRAS ===
    public void ChangeShadowQuality(int direction)
    {
        shadowQualityIndex = Mathf.Clamp(shadowQualityIndex + direction, 0, shadowQualityOptions.Length - 1);
        QualitySettings.shadowResolution = (ShadowResolution)shadowQualityIndex;
        UpdateUI();
        UserEventLogger.Instance.LogEvent("shadow_quality_changed", shadowQualityText.text);
    }

    // === REFLEJOS ===
    public void ToggleReflections()
    {
        reflectionsEnabled = !reflectionsEnabled;
        QualitySettings.realtimeReflectionProbes = reflectionsEnabled;
        UpdateUI();
        UserEventLogger.Instance.LogEvent("reflections_changed", reflectionsEnabled.ToString());
    }

    // === FOVEATED RENDERING ===
    public void ChangeFoveatedRendering(int direction)
    {
        foveatedIndex = Mathf.Clamp(foveatedIndex + direction, 0, foveatedOptions.Length - 1);
        UpdateUI();
        UserEventLogger.Instance.LogEvent("foveated_rendering_changed", foveatedRenderingText.text);
    }

    // === OCCLUSION CULLING ===
    public void ToggleOcclusionCulling()
    {
        occlusionCullingEnabled = !occlusionCullingEnabled;
        Camera.main.useOcclusionCulling = occlusionCullingEnabled;
        UpdateUI();
        UserEventLogger.Instance.LogEvent("occlusion_culling_changed", occlusionCullingEnabled.ToString());
    }

    // === EVENTOS DE POINTER ===
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == brightnessSlider.gameObject)
        {
            isAdjustingBrightness = true;
        }
        else if (eventData.pointerEnter == contrastSlider.gameObject)
        {
            isAdjustingContrast = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isAdjustingBrightness)
        {
            UserEventLogger.Instance.LogEvent("brightness_changed", brightnessText.text);
            isAdjustingBrightness = false;
        }
        if (isAdjustingContrast)
        {
            UserEventLogger.Instance.LogEvent("contrast_changed", contrastText.text);
            isAdjustingContrast = false;
        }
    }

    // === ACTUALIZAR INTERFAZ ===
    void UpdateUI()
    {
        qualityText.text = qualityOptions[qualityIndex];
        renderDistanceText.text = renderDistanceOptions[renderDistanceIndex];
        shadowQualityText.text = shadowQualityOptions[shadowQualityIndex];
        reflectionsText.text = reflectionsEnabled ? "Sí" : "No";
        foveatedRenderingText.text = foveatedOptions[foveatedIndex];
        occlusionCullingText.text = occlusionCullingEnabled ? "Sí" : "No";
    }

    // === GUARDAR Y CARGAR AJUSTES ===
    void LoadSettings()
    {
        qualityIndex = PlayerPrefs.GetInt("Quality", 2);
        QualitySettings.SetQualityLevel(qualityIndex);

        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 50);
        contrastSlider.value = PlayerPrefs.GetFloat("Contrast", 50);

        renderDistanceIndex = PlayerPrefs.GetInt("RenderDistance", 2);
        shadowQualityIndex = PlayerPrefs.GetInt("ShadowQuality", 2);
        reflectionsEnabled = PlayerPrefs.GetInt("Reflections", 1) == 1;
        foveatedIndex = PlayerPrefs.GetInt("Foveated", 0);
        occlusionCullingEnabled = PlayerPrefs.GetInt("OcclusionCulling", 1) == 1;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.SetInt("RenderDistance", renderDistanceIndex);
        PlayerPrefs.SetInt("ShadowQuality", shadowQualityIndex);
        PlayerPrefs.SetInt("Reflections", reflectionsEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Foveated", foveatedIndex);
        PlayerPrefs.SetInt("OcclusionCulling", occlusionCullingEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}