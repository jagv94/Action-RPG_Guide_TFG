using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using System;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Unity.XR.CoreUtils;
using UnityEngine.UI;

public class Locomotion : MonoBehaviour
{
    private enum MovementMode { Teleport = 0, Continuous = 1, Hybrid = 2 }
    private enum MoveSpeedLevel { Slow = 0, Medium = 1, Fast = 2 }
    private enum RotationMode { Disabled = 0, Snap = 1 }
    private enum VignetteRestrictionLevel { Disabled = 0, Low = 1, Medium = 2, High = 3 }
    private enum HeightMode { Automatic = 0, Manual = 1 }

    private readonly float[] speedValues = { 1.5f, 2.5f, 4f };
    private readonly float[] restrictionValues = { 1.0f, 0.75f, 0.7f, 0.65f };

    public TunnelingVignetteController vignetteController;
    public TextMeshProUGUI movementText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI restrictionText;
    public TextMeshProUGUI rotationText;
    public TextMeshProUGUI heightText;
    public TextMeshProUGUI heightCmText;

    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private GameObject heightSliderPanel;
    [SerializeField] private Slider heightSlider;

    private MovementMode currentMode;
    private MoveSpeedLevel currentSpeedLevel;
    private VignetteRestrictionLevel currentRestrictionLevel;
    private RotationMode currentRotationMode;
    private HeightMode currentHeightMode;

    private ContinuousMoveProvider continuousMove;
    private DynamicMoveProvider dynamicMoveProvider;
    private TeleportationProvider teleportationProvider;
    private SnapTurnProvider snapTurnProvider;
    private ContinuousTurnProvider continuousTurnProvider;

    [SerializeField] private XRRayInteractor teleportInteractor;
    private XRInteractorLineVisual lineVisual;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();
        teleportationProvider = FindFirstObjectByType<TeleportationProvider>();
        snapTurnProvider = FindFirstObjectByType<SnapTurnProvider>();
        continuousTurnProvider = FindFirstObjectByType<ContinuousTurnProvider>();
        dynamicMoveProvider = FindFirstObjectByType<DynamicMoveProvider>();

        if (teleportInteractor != null)
        {
            lineVisual = teleportInteractor.GetComponent<XRInteractorLineVisual>();
        }
    }

    private void Start()
    {
        currentMode = (MovementMode)PlayerPrefs.GetInt("LocomotionMode", 0);
        SetMovementMode(currentMode);

        currentSpeedLevel = (MoveSpeedLevel)PlayerPrefs.GetInt("SpeedLevel", 1); // Media por defecto
        SetMoveSpeed(currentSpeedLevel);

        currentRestrictionLevel = (VignetteRestrictionLevel)PlayerPrefs.GetInt("RestrictionLevel", 0);
        SetRestrictionLevel(currentRestrictionLevel);

        currentRotationMode = (RotationMode)PlayerPrefs.GetInt("RotationMode", 1);
        SetRotationMode(currentRotationMode);

        currentHeightMode = (HeightMode)PlayerPrefs.GetInt("HeightMode", (int)HeightMode.Automatic);
        SetHeightMode(currentHeightMode);

        float savedHeight = PlayerPrefs.GetFloat("ManualHeight", xrOrigin.CameraYOffset);
        heightSlider.value = Mathf.Clamp(savedHeight, 0.5f, 2f);
        UpdateHeightText(savedHeight);
        UpdateHeight(savedHeight);
    }

    private void Update()
    {
        if (currentHeightMode == HeightMode.Automatic)
        {
            AutoDetectHeight();
            UpdateHeightText(xrOrigin.CameraYOffset);
        }
    }

#region MovementMode
    private void SetMovementMode(MovementMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case MovementMode.Hybrid:
                SetHybridMode();
                break;
            case MovementMode.Teleport:
                SetTeleportationMode();
                break;
            case MovementMode.Continuous:
                SetContinuousMode();
                break;
        }

        PlayerPrefs.SetInt("LocomotionMode", (int)mode);
        PlayerPrefs.Save();
    }

    private void SetTeleportationMode()
    {
        movementText.SetText("Teletransporte");
        continuousMove.enabled = false;
        EnableTeleportation(true);
    }

    private void SetContinuousMode()
    {
        movementText.SetText("Locomoción Continua");
        continuousMove.enabled = true;
        EnableTeleportation(false);
    }

    private void SetHybridMode()
    {
        movementText.SetText("Híbrido");
        continuousMove.enabled = true;
        EnableTeleportation(true);
    }

    private void EnableTeleportation(bool isEnabled)
    {
        if (teleportationProvider != null)
        {
            teleportationProvider.enabled = isEnabled;
        }

        if (teleportInteractor != null)
        {
            teleportInteractor.allowSelect = isEnabled; // Permite o impide la selección de áreas de teletransporte

            if (lineVisual != null)
            {
                lineVisual.enabled = isEnabled; // Muestra u oculta la línea predictiva
            }
        }
    }

    public void OnMovementModeChange(int direction)
    {
        var modes = (MovementMode[])Enum.GetValues(typeof(MovementMode));
        int currentIndex = Array.IndexOf(modes, currentMode);
        int newIndex = (currentIndex + direction + modes.Length) % modes.Length;
        SetMovementMode(modes[newIndex]);
    }
#endregion MovementMode

#region MovementSpeed
    private void SetMoveSpeed(MoveSpeedLevel speedLevel)
    {
        currentSpeedLevel = speedLevel;

        if (dynamicMoveProvider != null)
        {
            dynamicMoveProvider.moveSpeed = speedValues[(int)speedLevel];
        }

        // Mostrar la velocidad seleccionada en pantalla
        switch (speedLevel)
        {
            case MoveSpeedLevel.Slow:
                speedText.SetText("Lenta");
                break;
            case MoveSpeedLevel.Medium:
                speedText.SetText("Media");
                break;
            case MoveSpeedLevel.Fast:
                speedText.SetText("Rápida");
                break;
        }

        // Guardar la configuración
        PlayerPrefs.SetInt("SpeedLevel", (int)speedLevel);
        PlayerPrefs.Save();
    }

    public void OnSpeedLevelChange(int direction)
    {
        var levels = (MoveSpeedLevel[])Enum.GetValues(typeof(MoveSpeedLevel));
        int currentIndex = Array.IndexOf(levels, currentSpeedLevel);
        int newIndex = (currentIndex + direction + levels.Length) % levels.Length;
        SetMoveSpeed(levels[newIndex]);
    }
#endregion MovementSpeed

#region VignetteRestriction
    private void SetRestrictionLevel(VignetteRestrictionLevel level)
    {
        currentRestrictionLevel = level;

        vignetteController.defaultParameters.apertureSize = restrictionValues[(int)level];

        switch (level)
        {
            case VignetteRestrictionLevel.Disabled:
                restrictionText.SetText("Desactivada");
                break;
            case VignetteRestrictionLevel.Low:
                restrictionText.SetText("Baja");
                break;
            case VignetteRestrictionLevel.Medium:
                restrictionText.SetText("Media");
                break;
            case VignetteRestrictionLevel.High:
                restrictionText.SetText("Alta");
                break;
        }

        PlayerPrefs.SetInt("RestrictionLevel", (int)level);
        PlayerPrefs.Save();
    }

    public void OnRestrictionLevelChange(int direction)
    {
        var levels = (VignetteRestrictionLevel[])Enum.GetValues(typeof(VignetteRestrictionLevel));
        int currentIndex = Array.IndexOf(levels, currentRestrictionLevel);
        int newIndex = (currentIndex + direction + levels.Length) % levels.Length;
        SetRestrictionLevel(levels[newIndex]);
    }
#endregion VignetteRestriction

#region CameraRotation
    private void SetRotationMode(RotationMode mode)
    {
        currentRotationMode = mode;

        switch (mode)
        {
            case RotationMode.Disabled:
                rotationText.SetText("Desactivada");
                snapTurnProvider.enabled = false;
                continuousTurnProvider.enabled = false;
                break;
            case RotationMode.Snap:
                rotationText.SetText("Incremental (Snap)");
                snapTurnProvider.enabled = true;
                continuousTurnProvider.enabled = false;
                break;
        }

        PlayerPrefs.SetInt("RotationMode", (int)mode);
        PlayerPrefs.Save();
    }

    public void OnRotationModeChange(int direction)
    {
        var modes = (RotationMode[])Enum.GetValues(typeof(RotationMode));
        int currentIndex = Array.IndexOf(modes, currentRotationMode);
        int newIndex = (currentIndex + direction + modes.Length) % modes.Length;
        SetRotationMode(modes[newIndex]);
    }
#endregion CameraRotation

#region PlayerHeight
    private void SetHeightMode(HeightMode mode)
    {
        currentHeightMode = mode;

        switch (mode)
        {
            case HeightMode.Automatic:
                heightSliderPanel.SetActive(false);
                AutoDetectHeight(); // Detectar altura automáticamente
                heightText.SetText("Automático");
                UpdateHeightText(xrOrigin.CameraYOffset);
                break;
            case HeightMode.Manual:
                heightSliderPanel.SetActive(true);
                heightText.SetText("Manual");

                // Sincronizar el valor del slider con el valor real en metros
                heightSlider.value = xrOrigin.CameraYOffset;

                UpdateHeight(xrOrigin.CameraYOffset);
                break;
        }

        // Guardar el valor en PlayerPrefs
        PlayerPrefs.SetInt("HeightMode", (int)mode);
        PlayerPrefs.Save();
    }

    private void AutoDetectHeight()
    {
        if (xrOrigin != null && xrOrigin.Camera != null)
        {
            // Detectar automáticamente la altura en metros
            float detectedHeight = xrOrigin.Camera.transform.localPosition.y;

            // Limitar el valor detectado a un rango razonable (por ejemplo, 0.5 m a 2 m)
            detectedHeight = Mathf.Clamp(detectedHeight, 0.5f, 2f);

            // Asignar el valor al XR Origin y sincronizar el slider
            xrOrigin.CameraYOffset = detectedHeight;
            heightSlider.value = detectedHeight; // Sincronizar valor real en metros
            UpdateHeightText(detectedHeight);
        }
    }

    public void UpdateHeight(float value)
    {
        if (xrOrigin != null)
        {
            xrOrigin.CameraYOffset = value;
            UpdateHeightText(value);

            PlayerPrefs.SetFloat("ManualHeight", value);
            PlayerPrefs.Save();
        }
    }

    private void UpdateHeightText(float value)
    {
        if (heightCmText != null)
        {
            int heightInCm = Mathf.RoundToInt(value * 100); // Convertir metros a cm
            heightCmText.SetText($"{heightInCm} cm");
        }
    }

    public void OnHeightModeChange(int direction)
    {
        var modes = (HeightMode[])Enum.GetValues(typeof(HeightMode));
        int currentIndex = Array.IndexOf(modes, currentHeightMode);
        int newIndex = (currentIndex + direction + modes.Length) % modes.Length;
        SetHeightMode(modes[newIndex]);
    }
#endregion PlayerHeight
}