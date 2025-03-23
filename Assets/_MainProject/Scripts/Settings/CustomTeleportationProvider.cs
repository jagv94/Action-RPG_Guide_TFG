using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using static Locomotion;

public class CustomTeleportationProvider : TeleportationProvider
{
    private XROrigin xrOrigin;
    private TunnelingVignetteController vignette;
    private bool isTeleporting = false;
    private MovementMode currentTeleportMode = MovementMode.Teleport;

    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float smoothTeleportSpeed = 5f;

    protected override void Awake()
    {
        base.Awake();
        vignette = FindFirstObjectByType<TunnelingVignetteController>();
        xrOrigin = FindFirstObjectByType<XROrigin>();
    }

    public void SetTeleportMode(MovementMode mode)
    {
        Debug.Log($"Modo de teletransporte actualizado a: {mode}");
        currentTeleportMode = mode;
    }

    public override bool QueueTeleportRequest(TeleportRequest teleportRequest)
    {
        if (isTeleporting)
        {
            Debug.LogWarning("Intento de teletransporte mientras otro está en curso.");
            return false;
        }

        Debug.Log($"QueueTeleportRequest recibido con destino: {teleportRequest.destinationPosition}");

        switch (currentTeleportMode)
        {
            case MovementMode.SmoothTeleport:
                Debug.Log("Ejecutando SmoothTeleport.");
                StartCoroutine(SmoothTeleport(teleportRequest.destinationPosition));
                return true; // No llamamos al TeleportationProvider base

            case MovementMode.BlinkTeleport:
                Debug.Log("Ejecutando BlinkTeleport.");
                StartCoroutine(BlinkTeleport(teleportRequest.destinationPosition));
                return true;

            default:
                Debug.Log("Ejecutando teletransporte normal.");
                return base.QueueTeleportRequest(teleportRequest);
        }
    }

    private IEnumerator SmoothTeleport(Vector3 targetPosition)
    {
        if (xrOrigin == null)
        {
            Debug.LogError("XROrigin no está asignado en CustomTeleportationProvider.");
            yield break;
        }

        isTeleporting = true; // Bloqueamos teletransporte mientras se interpola

        Vector3 startPosition = xrOrigin.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / smoothTeleportSpeed;
        float elapsedTime = 0f;

        Debug.Log($"Iniciando SmoothTeleport desde {startPosition} hasta {targetPosition}, duración: {duration}s");

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            xrOrigin.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;

            Debug.Log($"Interpolando... {xrOrigin.transform.position}");
        }

        xrOrigin.transform.position = targetPosition; // Asegurar posición final exacta

        Debug.Log("SmoothTeleport completado.");
        isTeleporting = false; // Desbloqueamos teletransporte
    }

    private IEnumerator BlinkTeleport(Vector3 targetPosition)
    {
        isTeleporting = true;

        if (vignette != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                vignette.currentParameters.apertureSize = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            vignette.currentParameters.apertureSize = 0f;
        }

        // Mover al usuario instantáneamente
        xrOrigin.transform.position = targetPosition;

        yield return new WaitForSeconds(0.1f);

        // Restaurar visibilidad
        if (vignette != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                vignette.currentParameters.apertureSize = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            vignette.currentParameters.apertureSize = 1f;
        }

        isTeleporting = false;
    }
}