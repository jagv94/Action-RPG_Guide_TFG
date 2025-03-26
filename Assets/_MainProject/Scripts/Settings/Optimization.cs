using UnityEngine;
using TMPro;

public class Optimization : MonoBehaviour
{
    public TMP_Text frameRateText;
    private readonly int[] framerates = { 75, 90, 120 };
    private int frameRateIndex;

    public TMP_Text dynamicResolutionText;
    private bool dynamicResolutionEnabled;

    public TMP_Text textureQualityText;
    private readonly string[] textureOptions = { "Equilibrada", "Alta Calidad", "Optimizada" };
    private int textureQualityIndex;

    public TMP_Text particleLimitText;
    private bool particleLimitEnabled;

    public TMP_Text postProcessingText;
    private bool postProcessingReduced;

    void Start()
    {
        LoadSettings();
        UpdateUI();
    }

    public void ChangeFramerate(int direction)
    {
        frameRateIndex = Mathf.Clamp(frameRateIndex + direction, 0, framerates.Length - 1);
        Application.targetFrameRate = framerates[frameRateIndex];
        PlayerPrefs.SetInt("Framerate", frameRateIndex);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ToggleDynamicResolution()
    {
        dynamicResolutionEnabled = !dynamicResolutionEnabled;
        ScalableBufferManager.ResizeBuffers(dynamicResolutionEnabled ? 1f : 0.75f, dynamicResolutionEnabled ? 1f : 0.75f);
        PlayerPrefs.SetInt("DynamicResolution", dynamicResolutionEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ChangeTextureQuality(int direction)
    {
        textureQualityIndex = Mathf.Clamp(textureQualityIndex + direction, 0, textureOptions.Length - 1);
        QualitySettings.globalTextureMipmapLimit = textureQualityIndex == 2 ? 2 : textureQualityIndex == 1 ? 0 : 1;
        PlayerPrefs.SetInt("TextureQuality", textureQualityIndex);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void ToggleParticleLimit()
    {
        particleLimitEnabled = !particleLimitEnabled;
        ParticleSystem[] particles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var particle in particles)
        {
            var main = particle.main;
            main.maxParticles = particleLimitEnabled ? 200 : 1000;
        }
        PlayerPrefs.SetInt("ParticleLimit", particleLimitEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void TogglePostProcessingReduction()
    {
        postProcessingReduced = !postProcessingReduced;
        var postProcessingVolume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();
        if (postProcessingVolume != null)
        {
            postProcessingVolume.enabled = !postProcessingReduced;
        }
        PlayerPrefs.SetInt("PostProcessing", postProcessingReduced ? 1 : 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    void UpdateUI()
    {
        frameRateText.text = $"{framerates[frameRateIndex]}Hz";
        dynamicResolutionText.text = dynamicResolutionEnabled ? "Activado" : "Desactivado";
        textureQualityText.text = textureOptions[textureQualityIndex];
        particleLimitText.text = particleLimitEnabled ? "Sí" : "No";
        postProcessingText.text = postProcessingReduced ? "Sí" : "No";
    }

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
}