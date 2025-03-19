using UnityEngine;
using TMPro;

public class Optimization : MonoBehaviour
{
    // Framerate
    public TMP_Text frameRateText;
    private readonly int[] framerates = { 75, 90, 120 };
    private int frameRateIndex = 1;

    // Resolución dinámica
    public TMP_Text dynamicResolutionText;
    private bool dynamicResolutionEnabled = true;

    // Carga de texturas
    public TMP_Text textureQualityText;
    private readonly string[] textureOptions = { "Equilibrada", "Alta Calidad", "Optimizada" };
    private int textureQualityIndex = 0;

    // Partículas
    public TMP_Text particleLimitText;
    private bool particleLimitEnabled = false;

    // Reducción de postprocesado
    public TMP_Text postProcessingText;
    private bool postProcessingReduced = false;

    void Start()
    {
        LoadSettings();
        UpdateUI();
    }

    // === FRAMERATE ===
    public void ChangeFramerate(int direction)
    {
        frameRateIndex = Mathf.Clamp(frameRateIndex + direction, 0, framerates.Length - 1);
        Application.targetFrameRate = framerates[frameRateIndex];
        UpdateUI();
        UserEventLogger.Instance.LogEvent("framerate_changed", frameRateText.text);
    }

    // === RESOLUCIÓN DINÁMICA ===
    public void ToggleDynamicResolution()
    {
        dynamicResolutionEnabled = !dynamicResolutionEnabled;
        ScalableBufferManager.ResizeBuffers(dynamicResolutionEnabled ? 1f : 0.75f, dynamicResolutionEnabled ? 1f : 0.75f);
        UpdateUI();
        UserEventLogger.Instance.LogEvent("dynamic_resolution_changed", dynamicResolutionEnabled.ToString());
    }

    // === CARGA DE TEXTURAS ===
    public void ChangeTextureQuality(int direction)
    {
        textureQualityIndex = Mathf.Clamp(textureQualityIndex + direction, 0, textureOptions.Length - 1);
        QualitySettings.globalTextureMipmapLimit = textureQualityIndex == 2 ? 2 : textureQualityIndex == 1 ? 0 : 1;
        UpdateUI();
        UserEventLogger.Instance.LogEvent("texture_quality_changed", textureQualityText.text);
    }

    // === LIMITACIÓN DE PART�CULAS ===
    public void ToggleParticleLimit()
    {
        particleLimitEnabled = !particleLimitEnabled;
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var particle in particles)
        {
            var main = particle.main;
            main.maxParticles = particleLimitEnabled ? 200 : 1000;
        }
        UpdateUI();
        UserEventLogger.Instance.LogEvent("particle_limit_changed", particleLimitEnabled.ToString());
    }

    // === REDUCCIÓN DE POSTPROCESADO ===
    public void TogglePostProcessingReduction()
    {
        postProcessingReduced = !postProcessingReduced;
        var postProcessingVolume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();
        if (postProcessingVolume != null)
        {
            postProcessingVolume.enabled = !postProcessingReduced;
        }
        UpdateUI();
        UserEventLogger.Instance.LogEvent("post_processing_changed", postProcessingReduced.ToString());
    }

    // === ACTUALIZAR INTERFAZ ===
    void UpdateUI()
    {
        frameRateText.text = $"{framerates[frameRateIndex]}Hz";
        dynamicResolutionText.text = dynamicResolutionEnabled ? "Activado" : "Desactivado";
        textureQualityText.text = textureOptions[textureQualityIndex];
        particleLimitText.text = particleLimitEnabled ? "S�" : "No";
        postProcessingText.text = postProcessingReduced ? "S�" : "No";
    }

    // === GUARDAR Y CARGAR AJUSTES ===
    void LoadSettings()
    {
        frameRateIndex = PlayerPrefs.GetInt("Framerate", 1);
        Application.targetFrameRate = framerates[frameRateIndex];

        dynamicResolutionEnabled = PlayerPrefs.GetInt("DynamicResolution", 1) == 1;
        ScalableBufferManager.ResizeBuffers(dynamicResolutionEnabled ? 1f : 0.75f, dynamicResolutionEnabled ? 1f : 0.75f);

        textureQualityIndex = PlayerPrefs.GetInt("TextureQuality", 0);
        QualitySettings.globalTextureMipmapLimit = textureQualityIndex == 2 ? 2 : textureQualityIndex == 1 ? 0 : 1;

        particleLimitEnabled = PlayerPrefs.GetInt("ParticleLimit", 0) == 1;

        postProcessingReduced = PlayerPrefs.GetInt("PostProcessing", 0) == 1;
        var postProcessingVolume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();
        if (postProcessingVolume != null)
        {
            postProcessingVolume.enabled = !postProcessingReduced;
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Framerate", frameRateIndex);
        PlayerPrefs.SetInt("DynamicResolution", dynamicResolutionEnabled ? 1 : 0);
        PlayerPrefs.SetInt("TextureQuality", textureQualityIndex);
        PlayerPrefs.SetInt("ParticleLimit", particleLimitEnabled ? 1 : 0);
        PlayerPrefs.SetInt("PostProcessing", postProcessingReduced ? 1 : 0);
        PlayerPrefs.Save();
    }
}