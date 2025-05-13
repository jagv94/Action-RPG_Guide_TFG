using UnityEngine;

public class PlatformTriggerController : MonoBehaviour
{
    public int totalSpheres = 4;                 // Número total de esferas necesarias
    public Transform destinationPoint;           // A dónde mover el suelo
    public float moveSpeed = 2f;
    public AudioSource soundEffect;

    private int currentActivations = 0;
    private bool isMoving = false;
    private Vector3 finalPosition;

    void Start()
    {
        if (destinationPoint != null)
        {
            finalPosition = destinationPoint.position;
        }
    }

    public void NotifySphereActivated()
    {
        currentActivations++;

        Debug.Log($"Esferas activadas: {currentActivations}/{totalSpheres}");

        if (currentActivations >= totalSpheres && !isMoving)
        {
            StartCoroutine(MovePlatform());
        }
    }

    private System.Collections.IEnumerator MovePlatform()
    {
        isMoving = true;
        soundEffect.Play();

        while (Vector3.Distance(transform.position, finalPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = finalPosition;
        isMoving = false;
    }
}