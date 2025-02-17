using UnityEngine;

public class SettingsTracker : MonoBehaviour
{
    public static SettingsTracker Instance { get; private set; }
    public int SettingsVisits { get; private set; } = 0;

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

    public void RegisterSettingsVisit()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        SettingsVisits++;
        UserEventLogger.Instance.LogEvent("settings_opened", "SettingsMenu", 0f);
    }
}