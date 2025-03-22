using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class CustomXRInteractorLineVisual : XRInteractorLineVisual
{
    private ILineRenderable lineRenderable;

    private void Start() // Usamos Start() en lugar de Awake()
    {
        lineRenderable = GetComponent<ILineRenderable>(); // Obtener referencia al sistema de líneas
    }

    public bool GetTeleportTarget(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;
        if (lineRenderable == null)
            return false;

        Vector3 normal;
        int positionInLine;
        bool isValidTarget;

        bool hit = lineRenderable.TryGetHitInfo(out targetPosition, out normal, out positionInLine, out isValidTarget);
        return hit && isValidTarget;
    }
}