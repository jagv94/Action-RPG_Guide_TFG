using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Locomotion : MonoBehaviour
{
    public enum MovementMode { Teleport, Continuous, Hybrid }

    public MovementMode currentMode;
    public TextMeshProUGUI movementText;
    private ContinuousMoveProvider continuousMove;

    [SerializeField] private InputActionManager inputActionManager;
    private InputAction teleportAction;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();

    }

    private void Start()
    {
        // Obtener la acción de teletransporte desde el InputActionManager
        teleportAction = inputActionManager.actionAssets[0].FindAction("XRI Right Locomotion/Teleport Mode");
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
        teleportAction.Enable(); // Habilitar teletransporte
    }

    private void SetTeleportationMode()
    {
        movementText.SetText("Teletransporte");
        continuousMove.enabled = false;
        teleportAction.Enable(); // Habilitar teletransporte
    }

    private void SetContinuousMode()
    {
        movementText.SetText("Locomoción Continua");
        continuousMove.enabled = true;
        teleportAction.Disable(); // Deshabilitar teletransporte
    }

    public void OnMovementModeChange(int direction)
    {
        int modeCount = System.Enum.GetValues(typeof(MovementMode)).Length;
        int newMode = ((int)currentMode + direction + modeCount) % modeCount;
        SetMovementMode((MovementMode)newMode);
    }
}