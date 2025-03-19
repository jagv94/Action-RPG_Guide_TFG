using System.Collections;
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

    private ContinuousMoveProvider continuousMove;
    private TeleportationProvider teleportation;
    private XROrigin xrOrigin;
    private TunnelingVignetteController vignette;

    private void Awake()
    {
        continuousMove = FindFirstObjectByType<ContinuousMoveProvider>();
        teleportation = FindFirstObjectByType<TeleportationProvider>();
        xrOrigin = FindFirstObjectByType<XROrigin>();
        vignette = FindFirstObjectByType<TunnelingVignetteController>();
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
        continuousMove.enabled = true;
        teleportation.enabled = true;
        Debug.Log("Modo híbrido activado");
    }

    private void SetTeleportationMode()
    {
        continuousMove.enabled = false;
        teleportation.enabled = true;
        Debug.Log("Modo teletransporte activado");
    }

    private void SetSmoothTeleportationMode()
    {
        continuousMove.enabled = false;
        teleportation.enabled = false;

        Vector3 targetPosition = new Vector3(0, 0, 5); // Ejemplo
        StartCoroutine(SmoothTeleport(targetPosition));
    }

    private IEnumerator SmoothTeleport(Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsedTime = 0;
        Vector3 startPosition = xrOrigin.transform.position;

        while (elapsedTime < duration)
        {
            xrOrigin.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        xrOrigin.transform.position = targetPosition;
    }

    private void SetContinuousMode()
    {
        continuousMove.enabled = true;
        teleportation.enabled = false;
        Debug.Log("Modo continuo activado");
    }

    // Teletransporte con parpadeo
    private void SetBlinkTeleportationMode()
    {
        continuousMove.enabled = false;
        teleportation.enabled = false;

        Vector3 targetPosition = new Vector3(0, 0, 5); // Ejemplo
        StartCoroutine(BlinkTeleport(targetPosition));
    }

    private IEnumerator BlinkTeleport(Vector3 targetPosition)
    {
        if (vignette == null)
        {
            Debug.LogWarning("TunnelingVignetteController no está asignado.");
            yield break;
        }

        // Oscurecer pantalla (cerrar viñeta)
        Debug.Log("Oscureciendo pantalla...");
        float duration = 0.3f;
        float elapsedTime = 0;
        float startAperture = vignette.currentParameters.apertureSize;
        float targetAperture = 0f; // Oscurecer completamente

        while (elapsedTime < duration)
        {
            vignette.defaultParameters.apertureSize = Mathf.Lerp(startAperture, targetAperture, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vignette.defaultParameters.apertureSize = targetAperture;

        // Teletransporte
        xrOrigin.transform.position = targetPosition;

        yield return new WaitForSeconds(0.1f);

        // Desvanecer (abrir viñeta)
        Debug.Log("Desvaneciendo pantalla...");
        elapsedTime = 0;
        startAperture = vignette.defaultParameters.apertureSize;
        targetAperture = 1f; // Abrir completamente

        while (elapsedTime < duration)
        {
            vignette.defaultParameters.apertureSize = Mathf.Lerp(startAperture, targetAperture, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vignette.defaultParameters.apertureSize = targetAperture;
    }

    public void OnModeChange(int mode)
    {
        SetMovementMode((MovementMode)mode);
    }
}
