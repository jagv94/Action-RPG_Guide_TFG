using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportTracker : MonoBehaviour
{
    public InputActionReference teleportAction;
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

    private void Update()
    {
        if (teleportAction.action.WasPressedThisFrame())
        {
            RegisterTeleport();
        }
    }

    public void RegisterTeleport()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        TeleportCount++;
        UserEventLogger.Instance.LogEvent("teleport", "Player", 0f);
    }
}