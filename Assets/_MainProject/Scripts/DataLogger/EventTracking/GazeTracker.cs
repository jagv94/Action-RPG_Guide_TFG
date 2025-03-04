using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeTracker : MonoBehaviour
{
    public static GazeTracker Instance;
    public Transform vrCamera; // Referencia a la cámara del visor VR
    public float gazeThreshold = 2.0f; // Tiempo en segundos para considerar que el usuario ha fijado la mirada

    private string currentGazeTarget = "None";
    private float gazeTime = 0.0f;
    private float gazeStartTime = 0.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        TrackGaze();
    }

    private void TrackGaze()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(vrCamera.position, vrCamera.forward, out hit))
        {
            string hitObjectName = hit.collider.gameObject.name;

            if (hitObjectName != currentGazeTarget)
            {
                RegisterGazeTargetChange(hitObjectName);
            }
            else
            {
                gazeTime = Time.time - gazeStartTime;
            }
        }
        else
        {
            RegisterGazeTargetChange("Air"); // Mirada al vacío
        }
    }

    private void RegisterGazeTargetChange(string newTarget)
    {
        if (currentGazeTarget != "None" && gazeTime >= gazeThreshold)
        {
            UserEventLogger.Instance.LogEvent("gaze_hold", currentGazeTarget, gazeTime);
        }

        currentGazeTarget = newTarget;
        gazeStartTime = Time.time;
        gazeTime = 0.0f;
    }

    public string GetCurrentGazeTarget()
    {
        return currentGazeTarget;
    }
}