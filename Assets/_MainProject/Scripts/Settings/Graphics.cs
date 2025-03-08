using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Graphics : MonoBehaviour
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
        brightnessText.text = brightnessSlider.value.ToString();
        UserEventLogger.Instance.LogEvent("brightness_changed", brightnessText.text);
    }

    // === CONTRASTE ===
    public void OnContrastChanged()
    {
        // Simulación de contraste (en shaders o postprocesado sería lo real)
        Shader.SetGlobalFloat("_Contrast", contrastSlider.value / contrastSlider.maxValue);
        PlayerPrefs.SetFloat("Contrast", contrastSlider.value);
        contrastText.text = contrastSlider.value.ToString();
        UserEventLogger.Instance.LogEvent("contrast_changed", contrastText.text);
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
        // Simulación de Foveated Rendering (implementación real depende del SDK)
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

        float brightness = PlayerPrefs.GetFloat("Brightness", 50);
        brightnessSlider.value = brightness;
        RenderSettings.ambientIntensity = brightness / 100f;

        float contrast = PlayerPrefs.GetFloat("Contrast", 50);
        contrastSlider.value = contrast;
        Shader.SetGlobalFloat("_Contrast", contrast / 100f);

        renderDistanceIndex = PlayerPrefs.GetInt("RenderDistance", 2);
        Camera.main.farClipPlane = new float[] { 50f, 150f, 300f }[renderDistanceIndex];

        shadowQualityIndex = PlayerPrefs.GetInt("ShadowQuality", 2);
        QualitySettings.shadowResolution = (ShadowResolution)shadowQualityIndex;

        reflectionsEnabled = PlayerPrefs.GetInt("Reflections", 1) == 1;
        QualitySettings.realtimeReflectionProbes = reflectionsEnabled;

        foveatedIndex = PlayerPrefs.GetInt("Foveated", 0);
        occlusionCullingEnabled = PlayerPrefs.GetInt("OcclusionCulling", 1) == 1;
        Camera.main.useOcclusionCulling = occlusionCullingEnabled;
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
