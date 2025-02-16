using UnityEngine;

public class TeleportTracker : MonoBehaviour
{
    public static TeleportTracker Instance { get; private set; }
    public int TeleportCount { get; private set; } = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterTeleport()
    {
        TeleportCount++;
        UserEventLogger.Instance.LogEvent("teleport", "Player", 0f);
    }
}