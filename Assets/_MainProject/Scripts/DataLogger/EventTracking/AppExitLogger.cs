using UnityEngine;

public class AppExitLogger : MonoBehaviour
{
    private static AppExitLogger instance;
    public static AppExitLogger Instance => instance;

    private const string LastSessionFlag = "LastSessionClosedProperly";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        CheckForAbruptExit();
        PlayerPrefs.SetInt(LastSessionFlag, 0); // 0 = sesi�n en curso, a�n no cerrada
        PlayerPrefs.Save();
    }

    void OnApplicationQuit()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        UserEventLogger.Instance.LogEvent("app_exit", "application", 0f);
        RegisterNormalExit();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            RegisterNormalExit();
        }
    }

    private void RegisterNormalExit()
    {
        PlayerPrefs.SetInt(LastSessionFlag, 1); // 1 = sesi�n cerrada correctamente
        PlayerPrefs.Save();
    }

    private void CheckForAbruptExit()
    {
        if (PlayerPrefs.GetInt(LastSessionFlag, 1) == 0)
        {
            Debug.LogWarning("Se detect� un cierre abrupto en la �ltima sesi�n.");

            // Registrar el cierre abrupto como un evento
            UserEventLogger.Instance.LogEvent(
                eventType: "rage_quit",
                targetObject: "Application",
                duration: 0
            );
        }
    }
}