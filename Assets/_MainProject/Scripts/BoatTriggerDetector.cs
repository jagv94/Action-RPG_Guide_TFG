using UnityEngine;

public class BoatTriggerDetector : MonoBehaviour
{
    public BoatMovement boatMovement;

    private void OnTriggerEnter(Collider other)
    {
        if (boatMovement != null)
        {
            boatMovement.OnPlayerEnteredTrigger(other);
        }
    }
}