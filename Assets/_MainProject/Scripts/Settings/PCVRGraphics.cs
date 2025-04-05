using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

[RequireComponent(typeof(Volume))]
public class PCVRGraphicsSettings : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text qualityText;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Text brightnessText;
    [SerializeField] private Slider contrastSlider;
    [SerializeField] private TMP_Text contrastText;
    [SerializeField] private TMP_Text renderDistanceText;
    [SerializeField] private TMP_Text shadowQualityText;
    [SerializeField] private TMP_Text reflectionsText;
    [SerializeField] private TMP_Text occlusionCullingText;

    [Header("Rendering Configuration")]
    [SerializeField] private UniversalRendererData standaloneRendererData;
    [SerializeField] private Camera vrCamera;
    [SerializeField] private VolumeProfile postProcessProfile;

    private Volume _postProcessVolume;
    private ColorAdjustments _colorAdjustments;
    private UniversalRenderPipelineAsset _urpAsset;

    private readonly string[] _qualityOptions = { "Baja", "Media", "Alta" };
    private readonly string[] _renderDistanceOptions = { "Cercana", "Media", "Lejana" };
    private readonly string[] _shadowQualityOptions = { "Baja", "Media", "Alta" };

    private int _currentQuality;
    private int _currentRenderDistance;
    private int _currentShadowQuality;
    private bool _reflectionsEnabled;
    private bool _occlusionCullingEnabled;
    private bool _isInitialized;

    private void Awake()
    {
        _postProcessVolume = GetComponent<Volume>();
        _urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (vrCamera == null)
        {
            vrCamera = Camera.main;
            Debug.LogWarning("VR Camera not explicitly assigned, using Camera.main");
        }

        if (postProcessProfile != null)
        {
            _postProcessVolume.profile = postProcessProfile;
        }
    }

    private void Start()
    {
        InitializeSystem();
        LoadSettings();
        ApplySettings();
        UpdateUI();
    }

    private void InitializeSystem()
    {
        if (_isInitialized) return;

        if (!CheckRequiredComponents())
        {
            Debug.LogError("Critical components missing! Disabling graphics system.");
            enabled = false;
            return;
        }

        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        contrastSlider.onValueChanged.AddListener(SetContrast);

        if (!_postProcessVolume.profile.TryGet(out _colorAdjustments))
        {
            Debug.LogWarning("ColorAdjustments not found in Post-Processing Volume");
        }

        _isInitialized = true;
    }

    private bool CheckRequiredComponents()
    {
        if (_postProcessVolume == null || vrCamera == null || _urpAsset == null || standaloneRendererData == null)
        {
            Debug.LogError("Missing components: " +
                         (_postProcessVolume == null ? "PostProcessVolume " : "") +
                         (vrCamera == null ? "VRCamera " : "") +
                         (standaloneRendererData == null ? "RendererData " : "") +
                         (_urpAsset == null ? "URPAsset" : ""));
            return false;
        }
        return true;
    }

    #region Public Control Methods
    public void ChangeQuality(int direction)
    {
        if (!_isInitialized) return;

        _currentQuality = Mathf.Clamp(_currentQuality + direction, 0, _qualityOptions.Length - 1);
        PlayerPrefs.SetInt("GraphicsQuality", _currentQuality);

        ApplySettings();
        UpdateUI();
    }


    public void ChangeRenderDistance(int direction)
    {
        if (!_isInitialized) return;

        _currentRenderDistance = Mathf.Clamp(_currentRenderDistance + direction, 0, _renderDistanceOptions.Length - 1);
        PlayerPrefs.SetInt("RenderDistance", _currentRenderDistance);

        ApplySettings();
        UpdateUI();
    }


    public void ChangeShadowQuality(int direction)
    {
        if (!_isInitialized) return;

        _currentShadowQuality = Mathf.Clamp(_currentShadowQuality + direction, 0, _shadowQualityOptions.Length - 1);
        PlayerPrefs.SetInt("ShadowQuality", _currentShadowQuality);

        ApplySettings();
        UpdateUI();
    }


    public void ToggleReflections()
    {
        if (!_isInitialized) return;

        _reflectionsEnabled = !_reflectionsEnabled;
        PlayerPrefs.SetInt("Reflections", _reflectionsEnabled ? 1 : 0);

        ApplySettings();
        UpdateUI();
    }

    public void ToggleOcclusionCulling()
    {
        if (!_isInitialized) return;

        _occlusionCullingEnabled = !_occlusionCullingEnabled;
        PlayerPrefs.SetInt("OcclusionCulling", _occlusionCullingEnabled ? 1 : 0);

        ApplySettings();
        UpdateUI();
    }
    #endregion

    #region Rendering Configuration
    private float GetRenderDistanceValue(int index)
    {
        return index switch
        {
            0 => 50f,   // Cercana
            1 => 150f,  // Media
            _ => 300f   // Lejana
        };
    }

    private float GetShadowDistanceValue(int index)
    {
        return index switch
        {
            0 => 50f,   // Baja
            1 => 100f,  // Media
            _ => 150f   // Alta
        };
    }

    private void ApplyQualityPreset()
    {
        switch (_currentQuality)
        {
            case 0: // Low
                _urpAsset.renderScale = 0.8f;
                _urpAsset.msaaSampleCount = 2;
                _urpAsset.shadowCascadeCount = 1;
                break;

            case 1: // Medium
                _urpAsset.renderScale = 1.0f;
                _urpAsset.msaaSampleCount = 4;
                _urpAsset.shadowCascadeCount = 2;
                break;

            case 2: // High
                _urpAsset.renderScale = 1.2f;
                _urpAsset.msaaSampleCount = 4;
                _urpAsset.shadowCascadeCount = 4;
                break;
        }
    }

    private void ConfigureSSR(bool enabled)
    {
        if (standaloneRendererData == null) return;

        foreach (var feature in standaloneRendererData.rendererFeatures)
        {
            if (feature != null && feature.GetType().Name.Contains("ScreenSpaceReflections"))
            {
                feature.SetActive(enabled);
                standaloneRendererData.SetDirty();
                return;
            }
        }
        Debug.LogWarning("Screen Space Reflections feature not found");
    }
    #endregion

    #region Post-Processing
    public void SetBrightness(float value)
    {
        if (_colorAdjustments != null)
        {
            _colorAdjustments.postExposure.value = Mathf.Lerp(-1f, 1f, value / 100f);
            PlayerPrefs.SetFloat("Brightness", value);
            brightnessText.text = value.ToString("F0");

            ApplySettings();
        }
    }

    public void SetContrast(float value)
    {
        if (_colorAdjustments != null)
        {
            _colorAdjustments.contrast.value = Mathf.Lerp(-30f, 30f, value / 100f);
            PlayerPrefs.SetFloat("Contrast", value);
            contrastText.text = value.ToString("F0");

            ApplySettings();
        }
    }
    #endregion

    #region Settings Management
    private void LoadSettings()
    {
        if (!_isInitialized) return;

        // Quality
        _currentQuality = PlayerPrefs.GetInt("GraphicsQuality", 1);

        // Post-Processing
        SetBrightness(PlayerPrefs.GetFloat("Brightness", 50f));
        SetContrast(PlayerPrefs.GetFloat("Contrast", 50f));

        // Render
        _currentRenderDistance = PlayerPrefs.GetInt("RenderDistance", 1);
        vrCamera.farClipPlane = GetRenderDistanceValue(_currentRenderDistance);

        // Shadows
        _currentShadowQuality = PlayerPrefs.GetInt("ShadowQuality", 1);
        _urpAsset.shadowDistance = GetShadowDistanceValue(_currentShadowQuality);

        // Effects
        _reflectionsEnabled = PlayerPrefs.GetInt("Reflections", 1) == 1;
        _occlusionCullingEnabled = PlayerPrefs.GetInt("OcclusionCulling", 1) == 1;
    }

    private void ApplySettings()
    {
        if (!_isInitialized) return;

        ApplyQualityPreset();

        vrCamera.farClipPlane = GetRenderDistanceValue(_currentRenderDistance);
        _urpAsset.shadowDistance = GetShadowDistanceValue(_currentShadowQuality);

        ConfigureSSR(_reflectionsEnabled);
        vrCamera.useOcclusionCulling = _occlusionCullingEnabled;
    }
    #endregion

    #region UI Update
    private void UpdateUI()
    {
        if (!_isInitialized) return;

        qualityText.text = _qualityOptions[_currentQuality];
        renderDistanceText.text = _renderDistanceOptions[_currentRenderDistance];
        shadowQualityText.text = _shadowQualityOptions[_currentShadowQuality];
        reflectionsText.text = _reflectionsEnabled ? "Sí" : "No";
        occlusionCullingText.text = _occlusionCullingEnabled ? "Sí" : "No";
    }
    #endregion

    #region Utility Methods
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
        ApplySettings();
        UpdateUI();
    }
    #endregion
}