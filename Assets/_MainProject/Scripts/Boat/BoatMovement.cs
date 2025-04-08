using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class BoatMovement : MonoBehaviour
{
    [Header("Asignaciones")]
    public Transform destinationPoint;          // Posición final a la que se moverá la barca
    public GameObject previewObject;            // Objeto de previsualización (se desactiva al iniciar)
    public BoatTriggerDetector triggerDetector; // Referencia al script del trigger

    [Header("Configuración")]
    public float moveSpeed = 3f;                // Velocidad de movimiento

    private bool isMoving = false;
    private Transform xrOrigin;
    private ContinuousMoveProvider moveProvider;
    private TeleportationProvider teleportationProvider;
    private Transform originalParent;

    private void Start()
    {
        if (previewObject != null)
            previewObject.SetActive(false);
    }

    public void OnPlayerEnteredTrigger(Collider other)
    {
        if (isMoving) return;

        if (other.transform.root.CompareTag("Player"))
        {
            xrOrigin = other.transform.root;
            originalParent = xrOrigin.parent;

            // Parenteamos al jugador a la barca
            xrOrigin.SetParent(transform);

            // Desactivar locomoción
            moveProvider = xrOrigin.GetComponentInChildren<ContinuousMoveProvider>();
            teleportationProvider = xrOrigin.GetComponentInChildren<TeleportationProvider>();

            if (moveProvider != null) moveProvider.enabled = false;
            if (teleportationProvider != null) teleportationProvider.enabled = false;

            StartCoroutine(MoveBoat());
        }
    }

    System.Collections.IEnumerator MoveBoat()
    {
        isMoving = true;

        Vector3 finalPosition = destinationPoint.position; // Guardamos la posición global destino antes de mover la barca

        while (Vector3.Distance(transform.position, finalPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = finalPosition;
        isMoving = false;

        // Reestablecer el parent original
        if (xrOrigin != null)
            xrOrigin.SetParent(originalParent);

        // Reactivar locomoción
        if (moveProvider != null) moveProvider.enabled = true;
        if (teleportationProvider != null) teleportationProvider.enabled = true;

        // Desactivar el trigger
        if (triggerDetector != null && triggerDetector.TryGetComponent<Collider>(out var triggerCollider))
        {
            triggerCollider.enabled = false;
        }
    }
}