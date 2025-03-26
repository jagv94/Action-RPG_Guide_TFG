using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Graphics : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public TMP_Text qualityText;
    private readonly string[] qualityOptions = { "Baja", "Media", "Alta" };
    private int qualityIndex;

    public Slider brightnessSlider;
    public TMP_Text brightnessText;
    public Slider contrastSlider;
    public TMP_Text contrastText;

    public TMP_Text renderDistanceText;
    private readonly string[] renderDistanceOptions = { "Cercana", "Media", "Lejana" };
    private int renderDistanceIndex;

    public TMP_Text shadowQualityText;
    private readonly string[] shadowQualityOptions = { "Baja", "Media", "Alta" };
    private int shadowQualityIndex;

    public TMP_Text reflectionsText;
    private bool reflectionsEnabled;

    public TMP_Text foveatedRenderingText;
    private readonly string[] foveatedOptions = { "Desactivado", "Estático", "Dinámico" };
    private int foveatedIndex;

    public TMP_Text occlusionCullingText;
    private bool occlusionCullingEnabled;

    private bool isAdjustingBrightness = false;
    private bool isAdjustingContrast = false;

    void Start()
    {
        LoadSettings();
        UpdateUI();
    }

    public void ChangeQuality(int direction)
    {
        qualityIndex = Mathf.Clamp(qualityIndex + direction, 0, qualityOptions.Length - 1);
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void OnBrightnessChanged()
    {
        RenderSettings.ambientIntensity = brightnessSlider.value / brightnessSlider.maxValue;
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        PlayerPrefs.Save();
        brightnessText.text = brightnessSlider.value.ToString("F0");
    }

    public void OnContrastChanged()
    {
        Shader.SetGlobalFloat("_Contrast", contrastSlider.value / contrastSlider.maxValue);
        PlayerPrefs.SetFloat("Contrast", contrastSlider.value);
        PlayerPrefs.Save();
        contrastText.text = contrastSlider.value.ToString("F0");
    }

    public void ChangeRenderDistance(int direction)
    {
        renderDistanceIndex = Mathf.Clamp(renderDistanceIndex + direction, 0, renderDistanceOptions.Length - 1);
        float[] distances = { 50f, 150f, 300f };
        Camera.main.farClipPlane = distances[renderDistanceIndex];
        PlayerPrefs.SetInt("RenderDistance", renderDistanceIndex);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ChangeShadowQuality(int direction)
    {
        shadowQualityIndex = Mathf.Clamp(shadowQualityIndex + direction, 0, shadowQualityOptions.Length - 1);
        QualitySettings.shadowResolution = (ShadowResolution)shadowQualityIndex;
        PlayerPrefs.SetInt("ShadowQuality", shadowQualityIndex);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ToggleReflections()
    {
        reflectionsEnabled = !reflectionsEnabled;
        QualitySettings.realtimeReflectionProbes = reflectionsEnabled;
        PlayerPrefs.SetInt("Reflections", reflectionsEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ChangeFoveatedRendering(int direction)
    {
        foveatedIndex = Mathf.Clamp(foveatedIndex + direction, 0, foveatedOptions.Length - 1);
        PlayerPrefs.SetInt("Foveated", foveatedIndex);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ToggleOcclusionCulling()
    {
        occlusionCullingEnabled = !occlusionCullingEnabled;
        Camera.main.useOcclusionCulling = occlusionCullingEnabled;
        PlayerPrefs.SetInt("OcclusionCulling", occlusionCullingEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == brightnessSlider.gameObject)
            isAdjustingBrightness = true;
        else if (eventData.pointerEnter == contrastSlider.gameObject)
            isAdjustingContrast = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isAdjustingBrightness)
            isAdjustingBrightness = false;
        if (isAdjustingContrast)
            isAdjustingContrast = false;
    }

    void UpdateUI()
    {
        qualityText.text = qualityOptions[qualityIndex];
        renderDistanceText.text = renderDistanceOptions[renderDistanceIndex];
        shadowQualityText.text = shadowQualityOptions[shadowQualityIndex];
        reflectionsText.text = reflectionsEnabled ? "Sí" : "No";
        foveatedRenderingText.text = foveatedOptions[foveatedIndex];
        occlusionCullingText.text = occlusionCullingEnabled ? "Sí" : "No";
    }

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
}