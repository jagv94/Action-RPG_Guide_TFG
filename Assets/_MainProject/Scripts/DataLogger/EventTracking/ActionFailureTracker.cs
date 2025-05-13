using UnityEngine;

public class ActionFailureTracker : MonoBehaviour
{
    public static ActionFailureTracker Instance { get; private set; }
    public int FailedAttempts { get; private set; } = 0;

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

    public void RegisterFailedAction(string targetObject)
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        FailedAttempts++;
        UserEventLogger.Instance.LogEvent("failed_action", targetObject, 0f);
    }
}