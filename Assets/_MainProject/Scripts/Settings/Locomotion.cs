using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class Locomotion : MonoBehaviour
{
    public enum MovementMode
    {
        Teleport = 0,
        BlinkTeleport = 1,
        SmoothTeleport = 2,
        Continuous = 3,
        Hybrid = 4
    }

    public MovementMode currentMode;
    public TextMeshProUGUI movementText;
    [SerializeField] private float teleportSpeed = 5f;
    [SerializeField] private CustomXRInteractorLineVisual customLineVisual;
    [SerializeField] private XRRayInteractor rayInteractor;

    private ContinuousMoveProvider continuousMove;
    private TeleportationProvider teleportation;
    private CustomTeleportationProvider customTeleportation;

    // Flag para controlar si ya hay un proceso de teletransporte en curso
    private bool isTeleporting = false;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();
        teleportation = FindFirstObjectByType<TeleportationProvider>();
        customTeleportation = FindFirstObjectByType<CustomTeleportationProvider>();

        if (rayInteractor != null)
        {
            rayInteractor.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void Start()
    {
        SetMovementMode(currentMode);
    }

    private void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (currentMode == MovementMode.SmoothTeleport || currentMode == MovementMode.BlinkTeleport)
        {
            TryTeleport();
        }
    }

    public void SetMovementMode(MovementMode mode)
    {
        if (isTeleporting)
            return;

        currentMode = mode;

        // Cancela cualquier teletransporte pendiente para evitar ejecuciones automáticas
        if (teleportation != null)
        {
            teleportation.enabled = false;
            teleportation.enabled = true; // Reinicia el proveedor para eliminar solicitudes pendientes
        }

        if (customTeleportation != null)
        {
            customTeleportation.enabled = false;
            customTeleportation.enabled = true; // Elimina solicitudes pendientes en el custom provider
        }

        switch (mode)
        {
            case MovementMode.Hybrid:
                SetHybridMode();
                break;
            case MovementMode.Teleport:
                SetTeleportationMode();
                break;
            case MovementMode.SmoothTeleport:
                SetSmoothTeleportationMode();
                break;
            case MovementMode.Continuous:
                SetContinuousMode();
                break;
            case MovementMode.BlinkTeleport:
                SetBlinkTeleportationMode();
                break;
        }
    }

    private void SetHybridMode()
    {
        movementText.SetText("Híbrido");
        continuousMove.enabled = true;
        if (teleportation != null) teleportation.enabled = true; // Usamos el teletransporte normal
        Debug.Log("Modo híbrido activado");
    }

    private void SetTeleportationMode()
    {
        movementText.SetText("Teletransporte");
        continuousMove.enabled = false;
        if (teleportation != null) teleportation.enabled = true; // Usamos el teletransporte normal
        Debug.Log("Modo teletransporte activado");
    }

    private void SetSmoothTeleportationMode()
    {
        movementText.SetText("Teletransporte Suave");
        continuousMove.enabled = false;

        if (teleportation != null)
            teleportation.enabled = false; // Desactivar TeleportationProvider normal

        if (customTeleportation != null)
        {
            customTeleportation.enabled = true; // Activar CustomTeleportationProvider
            customTeleportation.SetTeleportMode(MovementMode.SmoothTeleport);
        }
    }


    private void SetContinuousMode()
    {
        movementText.SetText("Locomoción Continua");
        continuousMove.enabled = true;
        Debug.Log("Modo continuo activado");
    }

    private void SetBlinkTeleportationMode()
    {
        movementText.SetText("Teletransporte Fundido");
        continuousMove.enabled = false;
        
        if (teleportation != null)
            teleportation.enabled = false;

        if (customTeleportation != null)
        {
            customTeleportation.enabled = true;
            customTeleportation.SetTeleportMode(MovementMode.BlinkTeleport);
        }
    }

    public Vector3 GetTeleportTarget()
    {
        if (customLineVisual == null)
        {
            Debug.LogError("CustomXRInteractorLineVisual no está asignado.");
            return Vector3.zero;
        }

        if (customLineVisual.GetTeleportTarget(out Vector3 targetPosition))
        {
            Debug.Log($"Destino obtenido por GetTeleportTarget: {targetPosition}");
            return targetPosition;
        }

        Debug.LogWarning("No se obtuvo un destino válido de GetTeleportTarget.");
        return Vector3.zero;
    }

    public void TryTeleport()
    {
        if (isTeleporting)
            return;

        Vector3 targetPosition = GetTeleportTarget();

        if (targetPosition != Vector3.zero)
        {
            TeleportRequest teleportRequest = new TeleportRequest
            {
                destinationPosition = targetPosition
            };

            if (customTeleportation != null && customTeleportation.QueueTeleportRequest(teleportRequest))
            {
                Debug.Log($"Solicitud de teletransporte enviada a {targetPosition}");
                isTeleporting = true;
                StartCoroutine(ResetTeleporting());
            }
            else
            {
                Debug.LogWarning("El teletransporte no se pudo iniciar.");
            }
        }
        else
        {
            Debug.LogWarning("No hay un objetivo válido para teletransportarse.");
        }
    }

    private IEnumerator ResetTeleporting()
    {
        yield return new WaitForSeconds(0.1f);
        isTeleporting = false;
    }

    public void OnMovementModeChange(int direction)
    {
        // Si se está teletransportando, no se cambia el modo
        if (isTeleporting)
            return;

        int modeCount = System.Enum.GetValues(typeof(MovementMode)).Length;
        int newMode = ((int)currentMode + direction + modeCount) % modeCount;
        SetMovementMode((MovementMode)newMode);
    }
}