using UnityEngine;

public class LazyFollowMenu : MonoBehaviour
{
    public Transform playerCamera; // La c�mara del jugador (en VR suele ser el centro de seguimiento)
    public float followSpeed = 5.0f;
    public float rotationSpeed = 5.0f;
    public Vector3 offset = new Vector3(0, 0, 1.0f); // Ajusta para posicionar el men� frente a la c�mara

    void Update()
    {
        if (playerCamera != null)
        {
            // Calcular la posici�n objetivo con el offset en la direcci�n hacia adelante de la c�mara
            Vector3 targetPosition = playerCamera.position + playerCamera.forward * offset.z +
                                     playerCamera.right * offset.x +
                                     playerCamera.up * offset.y;

            // Interpolar suavemente la posici�n
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Interpolar suavemente la rotaci�n hacia la c�mara
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - playerCamera.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}