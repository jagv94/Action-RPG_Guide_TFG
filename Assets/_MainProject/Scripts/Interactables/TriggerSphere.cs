using UnityEngine;

public class TriggerSphere : MonoBehaviour
{
    [Header("Referencias")]
    public PlatformTriggerController platformController; // Suelo a notificar
    public Material activatedMaterial;                   // Material al activarse

    [Header("Rotación")]
    public float rotationSpeed = 45f; // grados por segundo

    private bool isActivated = false;
    private Renderer rend;
    private Material originalMaterial;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalMaterial = rend.material;
        }
    }

    private void Update()
    {
        if (isActivated)
        {
            // Rotar hacia la derecha (sobre el eje Y)
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        if (other.transform.root.CompareTag("Player"))
        {
            isActivated = true;
            Debug.Log($"{gameObject.name} activada por el jugador.");

            // Cambiar material
            if (rend != null && activatedMaterial != null)
                rend.material = activatedMaterial;

            // Notificar al controlador
            platformController?.NotifySphereActivated();

            // Desactivar el collider si no se debe reutilizar
            GetComponent<Collider>().enabled = false;
        }
    }
}