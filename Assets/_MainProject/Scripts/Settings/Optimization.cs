using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class Optimization : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text frameRateText;
    public TMP_Text dynamicResolutionText;
    public TMP_Text textureQualityText;
    public TMP_Text particleLimitText;
    public TMP_Text postProcessingText;

    [Header("Post Processing Volume")]
    public Volume postProcessingVolume; // Asignado desde el Inspector

    private readonly int[] framerates = { 75, 90, 120 };
    private int frameRateIndex;

    private bool dynamicResolutionEnabled;

    private readonly string[] textureOptions = { "Equilibrada", "Alta Calidad", "Optimizada" };
    private int textureQualityIndex;

    private bool particleLimitEnabled;
    private bool isPostProcessingEnabled;

    void Start()
    {
        LoadSettings();
        ApplySettings();
        UpdateUI();
    }

    public void ChangeFramerate(int direction)
    {
        frameRateIndex = Mathf.Clamp(frameRateIndex + direction, 0, framerates.Length - 1);
        Application.targetFrameRate = framerates[frameRateIndex];
        PlayerPrefs.SetInt("Framerate", frameRateIndex);
        SaveAndUpdate();
    }

    public void ToggleDynamicResolution()
    {
        dynamicResolutionEnabled = !dynamicResolutionEnabled;
        float scale = dynamicResolutionEnabled ? 1f : 0.75f;
        ScalableBufferManager.ResizeBuffers(scale, scale);
        PlayerPrefs.SetInt("DynamicResolution", dynamicResolutionEnabled ? 1 : 0);
        SaveAndUpdate();
    }

    public void ChangeTextureQuality(int direction)
    {
        textureQualityIndex = Mathf.Clamp(textureQualityIndex + direction, 0, textureOptions.Length - 1);
        QualitySettings.globalTextureMipmapLimit = textureQualityIndex switch
        {
            0 => 1, // Equilibrada
            1 => 0, // Alta Calidad
            2 => 2, // Optimizada
            _ => 1
        };
        PlayerPrefs.SetInt("TextureQuality", textureQualityIndex);
        SaveAndUpdate();
    }

    public void ToggleParticleLimit()
    {
        particleLimitEnabled = !particleLimitEnabled;
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);

        foreach (var particle in particles)
        {
            var main = particle.main;
            main.maxParticles = particleLimitEnabled ? 200 : 1000;

            if (particleLimitEnabled)
                particle.Clear(); // Limpia exceso si se baja límite
        }

        PlayerPrefs.SetInt("ParticleLimit", particleLimitEnabled ? 1 : 0);
        SaveAndUpdate();
    }

    public void TogglePostProcessing()
    {
        isPostProcessingEnabled = !isPostProcessingEnabled;

        if (postProcessingVolume != null)
        {
            postProcessingVolume.enabled = isPostProcessingEnabled;
        }

        PlayerPrefs.SetInt("PostProcessing", isPostProcessingEnabled ? 1 : 0);
        SaveAndUpdate();
    }

    private void SaveAndUpdate()
    {
        PlayerPrefs.Save();
        UpdateUI();
    }

    private void UpdateUI()
    {
        frameRateText.text = $"{framerates[frameRateIndex]}Hz";
        dynamicResolutionText.text = dynamicResolutionEnabled ? "Activado" : "Desactivado";
        textureQualityText.text = textureOptions[textureQualityIndex];
        particleLimitText.text = particleLimitEnabled ? "Sí" : "No";
        postProcessingText.text = isPostProcessingEnabled ? "Activo" : "Desactivado";
    }

    private void LoadSettings()
    {
        frameRateIndex = PlayerPrefs.GetInt("Framerate", 1);
        dynamicResolutionEnabled = PlayerPrefs.GetInt("DynamicResolution", 1) == 1;
        textureQualityIndex = PlayerPrefs.GetInt("TextureQuality", 0);
        particleLimitEnabled = PlayerPrefs.GetInt("ParticleLimit", 0) == 1;
        isPostProcessingEnabled = PlayerPrefs.GetInt("PostProcessing", 1) == 1; // Activo por defecto
    }

    private void ApplySettings()
    {
        Application.targetFrameRate = framerates[frameRateIndex];

        float scale = dynamicResolutionEnabled ? 1f : 0.75f;
        ScalableBufferManager.ResizeBuffers(scale, scale);

        QualitySettings.globalTextureMipmapLimit = textureQualityIndex switch
        {
            0 => 1,
            1 => 0,
            2 => 2,
            _ => 1
        };

        if (postProcessingVolume != null)
        {
            postProcessingVolume.enabled = isPostProcessingEnabled;
        }

        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var particle in particles)
        {
            var main = particle.main;
            main.maxParticles = particleLimitEnabled ? 200 : 1000;
        }
    }
}