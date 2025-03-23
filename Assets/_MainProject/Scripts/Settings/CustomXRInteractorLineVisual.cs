using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class CustomXRInteractorLineVisual : XRInteractorLineVisual
{
    private ILineRenderable lineRenderable;

    private void Start()
    {
        // Busca el XRRayInteractor y luego obtiene el ILineRenderable
        XRRayInteractor rayInteractor = GetComponent<XRRayInteractor>();
        if (rayInteractor != null)
        {
            lineRenderable = rayInteractor.GetComponent<ILineRenderable>();
        }

        if (lineRenderable == null)
        {
            Debug.LogWarning("ILineRenderable es NULL en CustomXRInteractorLineVisual. ¿Está configurado correctamente en el XRRayInteractor?");
        }
    }

    public bool GetTeleportTarget(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;
        if (lineRenderable == null)
        {
            Debug.LogWarning("lineRenderable es NULL en CustomXRInteractorLineVisual.");
            return false;
        }

        Vector3 normal;
        int positionInLine;
        bool isValidTarget;

        bool hit = lineRenderable.TryGetHitInfo(out targetPosition, out normal, out positionInLine, out isValidTarget);

        Debug.Log($"TryGetHitInfo resultado: hit = {hit}, isValidTarget = {isValidTarget}, targetPosition = {targetPosition}");

        if (hit && isValidTarget)
            return true;

        return false;
    }
}