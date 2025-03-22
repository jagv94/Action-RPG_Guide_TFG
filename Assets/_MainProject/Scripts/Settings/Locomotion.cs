using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
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

    private ContinuousMoveProvider continuousMove;
    private TeleportationProvider teleportation;
    private XROrigin xrOrigin;
    private TunnelingVignetteController vignette;
    private CustomXRInteractorLineVisual customLineVisual;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();
        teleportation = FindFirstObjectByType<TeleportationProvider>();
        xrOrigin = FindFirstObjectByType<XROrigin>();
        vignette = FindFirstObjectByType<TunnelingVignetteController>();
        customLineVisual = FindFirstObjectByType<CustomXRInteractorLineVisual>();
    }

    private void Start()
    {
        SetMovementMode(currentMode);
    }

    public void SetMovementMode(MovementMode mode)
    {
        currentMode = mode;
        StopAllCoroutines();

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
        movementText.SetText("Hibrido");
        continuousMove.enabled = true;
        if (teleportation != null) teleportation.enabled = true;
        Debug.Log("Modo híbrido activado");
    }

    private void SetTeleportationMode()
    {
        movementText.SetText("Teletransporte");
        continuousMove.enabled = false;
        if (teleportation != null) teleportation.enabled = true;
        Debug.Log("Modo teletransporte activado");
    }

    private void SetSmoothTeleportationMode()
    {
        movementText.SetText("Teletransporte Suave");
        continuousMove.enabled = false;
        if (teleportation != null) teleportation.enabled = false;

        Vector3 targetPosition = GetTeleportTarget();
        if (targetPosition != Vector3.zero)
        {
            StartCoroutine(SmoothTeleport(targetPosition));
        }
    }

    private IEnumerator SmoothTeleport(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        float distance = Vector3.Distance(xrOrigin.transform.position, targetPosition);
        float duration = distance / teleportSpeed; // Ajustar duración en base a la velocidad

        Vector3 startPosition = xrOrigin.transform.position;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            xrOrigin.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        xrOrigin.transform.position = targetPosition; // Asegurar la posición final exacta
    }

    private void SetContinuousMode()
    {
        movementText.SetText("Locomoción Continua");
        continuousMove.enabled = true;
        if (teleportation != null) teleportation.enabled = false;
        Debug.Log("Modo continuo activado");
    }

    // Teletransporte con parpadeo
    private void SetBlinkTeleportationMode()
    {
        movementText.SetText("Teletransporte Fundido");
        continuousMove.enabled = false;
        if (teleportation != null) teleportation.enabled = true;

        StartCoroutine(BlinkTeleport());
    }

    private IEnumerator BlinkTeleport()
    {
        if (vignette == null)
        {
            Debug.LogWarning("TunnelingVignetteController no está asignado.");
            yield break;
        }

        float duration = 0.3f;
        float elapsedTime = 0;
        float startAperture = vignette.currentParameters.apertureSize;
        float targetAperture = 0f;

        while (elapsedTime < duration)
        {
            vignette.defaultParameters.apertureSize = Mathf.Lerp(startAperture, targetAperture, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vignette.defaultParameters.apertureSize = targetAperture;

        Vector3 targetPosition = GetTeleportTarget();
        if (targetPosition != Vector3.zero)
        {
            TeleportRequest request = new TeleportRequest()
            {
                destinationPosition = targetPosition,
                matchOrientation = MatchOrientation.None
            };
            teleportation.QueueTeleportRequest(request);
        }

        yield return new WaitForSeconds(0.1f);

        elapsedTime = 0;
        startAperture = vignette.defaultParameters.apertureSize;
        targetAperture = 1f;

        while (elapsedTime < duration)
        {
            vignette.defaultParameters.apertureSize = Mathf.Lerp(startAperture, targetAperture, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vignette.defaultParameters.apertureSize = targetAperture;
    }

    public Vector3 GetTeleportTarget()
    {
        Vector3 targetPosition = Vector3.zero;
        if (customLineVisual != null && customLineVisual.GetTeleportTarget(out targetPosition))
        {
            return targetPosition;
        }
        return Vector3.zero;
    }

    public void OnMovementModeChange(int direction)
    {
        int modeCount = System.Enum.GetValues(typeof(MovementMode)).Length;
        int newMode = ((int)currentMode + direction + modeCount) % modeCount;
        SetMovementMode((MovementMode)newMode);
    }
}