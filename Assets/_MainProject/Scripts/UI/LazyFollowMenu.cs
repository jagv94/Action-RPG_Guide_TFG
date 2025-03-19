using UnityEngine;

public class LazyFollowMenu : MonoBehaviour
{
    public Transform playerCamera;
    public float followSpeed = 5.0f;
    public float rotationSpeed = 5.0f;
    public Vector3 offset = new Vector3(0, 0, 1.0f);
    public float distanceThreshold = 0.1f; // Distancia m�nima antes de actualizar la posici�n
    public float angleThreshold = 2.0f; // Diferencia m�nima en grados antes de actualizar la rotaci�n

    void Update()
    {
        if (playerCamera != null)
        {
            // Calcular la posici�n objetivo con el offset
            Vector3 targetPosition = playerCamera.position + playerCamera.forward * offset.z +
                                     playerCamera.right * offset.x +
                                     playerCamera.up * offset.y;

            // Solo actualizar posici�n si la distancia es mayor que el umbral
            if (Vector3.Distance(transform.position, targetPosition) > distanceThreshold)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }

            // Calcular la rotaci�n objetivo mirando hacia la c�mara
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - playerCamera.position);

            // Solo actualizar rotaci�n si la diferencia angular es mayor que el umbral
            if (Quaternion.Angle(transform.rotation, targetRotation) > angleThreshold)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}