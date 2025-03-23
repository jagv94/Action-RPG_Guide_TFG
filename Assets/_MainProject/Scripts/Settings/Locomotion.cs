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
    private XRRayInteractor teleportInteractor;
    private XRInteractorLineVisual lineVisual;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();
        teleportationProvider = FindFirstObjectByType<TeleportationProvider>();
        teleportInteractor = FindTeleportInteractor();

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

    private XRRayInteractor FindTeleportInteractor()
    {
        // Busca todos los XRRayInteractors en la escena
        XRRayInteractor[] interactors = FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);

        // Devuelve el primero que tenga la funcionalidad de teletransporte habilitada
        foreach (XRRayInteractor interactor in interactors)
        {
            if (interactor.allowSelect)
            {
                return interactor;
            }
        }

        // Si no se encuentra ninguno, devuelve null
        return null;
    }

    public void OnMovementModeChange(int direction)
    {
        int modeCount = System.Enum.GetValues(typeof(MovementMode)).Length;
        int newMode = ((int)currentMode + direction + modeCount) % modeCount;
        SetMovementMode((MovementMode)newMode);
    }
}