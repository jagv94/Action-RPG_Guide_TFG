using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class Locomotion : MonoBehaviour
{
    public enum MovementMode { Teleport, Continuous, Hybrid }

    public MovementMode currentMode;
    public TextMeshProUGUI movementText;
    private ContinuousMoveProvider continuousMove;
    private TeleportationProvider teleportationProvider;
    [SerializeField] private XRRayInteractor teleportInteractor;
    private XRInteractorLineVisual lineVisual;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();
        teleportationProvider = FindFirstObjectByType<TeleportationProvider>();

        if (teleportInteractor != null)
        {
            lineVisual = teleportInteractor.GetComponent<XRInteractorLineVisual>();
        }
    }

    private void Start()
    {
        SetMovementMode(currentMode);
    }

    public void SetMovementMode(MovementMode mode)
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
    }

    private void SetHybridMode()
    {
        movementText.SetText("Híbrido");
        continuousMove.enabled = true;
        EnableTeleportation(true);
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
        int modeCount = System.Enum.GetValues(typeof(MovementMode)).Length;
        int newMode = ((int)currentMode + direction + modeCount) % modeCount;
        SetMovementMode((MovementMode)newMode);
    }
}