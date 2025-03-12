using UnityEngine;

public class LazyFollowMenu : MonoBehaviour
{
    public Transform playerCamera; // La cámara del jugador (en VR suele ser el centro de seguimiento)
    public float followSpeed = 5.0f;
    public float rotationSpeed = 5.0f;
    public Vector3 offset = new Vector3(0, 0, 1.0f); // Ajusta para posicionar el menú frente a la cámara

    void Update()
    {
        if (playerCamera != null)
        {
            // Calcular la posición objetivo con el offset en la dirección hacia adelante de la cámara
            Vector3 targetPosition = playerCamera.position + playerCamera.forward * offset.z +
                                     playerCamera.right * offset.x +
                                     playerCamera.up * offset.y;

            // Interpolar suavemente la posición
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Interpolar suavemente la rotación hacia la cámara
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - playerCamera.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}